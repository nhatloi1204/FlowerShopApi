using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Enums;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlowerShop.API.Services.Concrete;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private const string ProductModelType = "Product";

    public OrderService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // =========================================================================
    // CUSTOMER: CREATE ORDER (TRANSACTION + CONCURRENCY STOCK CHECK)
    // =========================================================================
    public async Task<BaseResponse<OrderOutputResource>> CreateOrderAsync(long customerId, OrderCreateInputResource request)
    {
        if (request.Items == null || !request.Items.Any())
            return BaseResponse<OrderOutputResource>.Fail("Order must contain at least one item.");

        // Extract address to create Snapshot
        var address = await _context.CustomerAddresses
            .Include(a => a.Province)
            .Include(a => a.Ward)
            .FirstOrDefaultAsync(a => a.Id == request.ReceiveAddressId && a.CustomerId == customerId);

        if (address == null)
            return BaseResponse<OrderOutputResource>.Fail("Customer address not found.");

        var paymentMethod = await _context.PaymentMethods.AnyAsync(p => p.Id == request.PaymentMethodId);
        if (!paymentMethod)
            return BaseResponse<OrderOutputResource>.Fail("Payment method does not exist.");
        // Create JSON Snapshot of the address
        var addressSnapshot = new ShippingAddressSnapshot
        {
            ReceiveCustomerName = address.ReceiveCustomerName,
            ReceiveCustomerPhone = address.ReceiveCustomerPhone,
            FullAddress = address.Address,
            WardName = address.Ward?.Name ?? string.Empty,
            ProvinceName = address.Province?.Name ?? string.Empty
        };
        string shippingAddressJson = JsonSerializer.Serialize(addressSnapshot);

        // Start transaction to ensure atomicity of order creation and stock deduction
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                CustomerId = customerId,
                ReceiveAddressId = request.ReceiveAddressId,
                PaymentMethodId = request.PaymentMethodId,
                ReceiveDate = request.ReceiveDate,
                ReceiveTime = request.ReceiveTime,
                DeliveryMode = request.DeliveryMode,
                Description = request.Description,
                Status = OrderStatus.Pending,
                ShippingAddress = shippingAddressJson,
                CreatedAt = DateTime.UtcNow,
                AmountPaid = 0 // unpaid as default
            };

            decimal totalOrderPrice = 0;
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

            // Lock product rows to prevent race condition
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var itemInput in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == itemInput.ProductId);
                if (product == null || product.Status == ProductStatus.Discontinued)
                    return BaseResponse<OrderOutputResource>.Fail($"Product with ID {itemInput.ProductId} does not exist or has been discontinued.");

                if (product.StockQuantity < itemInput.Quantity)
                    return BaseResponse<OrderOutputResource>.Fail($"Product '{product.Name}' is out of stock or insufficient quantity in stock (Remaining: {product.StockQuantity}).");

                // Deduct stock quantity directly
                product.StockQuantity -= itemInput.Quantity;
                if (product.StockQuantity == 0)
                {
                    product.Status = ProductStatus.OutOfStock;
                }
                product.UpdatedAt = DateTime.UtcNow;

                // Get first image for the product to save in OrderItem snapshot
                var firstImage = await _context.Medias
                    .Where(m => m.ModelType == ProductModelType && m.ModelId == product.Id)
                    .OrderBy(m => m.OrderColumn)
                    .Select(m => m.FileName)
                    .FirstOrDefaultAsync();

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemInput.Quantity,
                    UnitPrice = product.Price ?? 0,
                    ProductName = product.Name,
                    ProductImage = firstImage
                };

                totalOrderPrice += orderItem.UnitPrice * orderItem.Quantity;
                order.OrderItems.Add(orderItem);
            }

            order.TotalPrice = totalOrderPrice;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BaseResponse<OrderOutputResource>.Ok(await BuildOrderOutput(order), "Đặt đơn hàng thành công.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================================
    // CUSTOMER: VIEW ORDER HISTORY (TIGHTLY BOUND TO CUSTOMER ID)
    // =========================================================================
    public async Task<BaseResponse<PagedList<OrderOutputResource>>> GetCustomerOrderHistoryAsync(long customerId, OrderQueryResource query)
    {
        var queryable = _context.Orders.Where(o => o.CustomerId == customerId).AsNoTracking();

        queryable = ApplyOrderFilters(queryable, query);

        var totalItems = await queryable.CountAsync();
        int page = query.Page > 0 ? query.Page : 1;
        int pageSize = query.PageSize > 0 ? query.PageSize : 12;

        var orders = await queryable
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var outputItems = new List<OrderOutputResource>();
        foreach (var order in orders)
        {
            outputItems.Add(await BuildOrderOutput(order));
        }

        var result = new PagedList<OrderOutputResource>
        {
            Items = outputItems,
            TotalItems = totalItems,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return BaseResponse<PagedList<OrderOutputResource>>.Ok(result, "Get customer order history successfully.");
    }

    public async Task<BaseResponse<OrderOutputResource>> GetCustomerOrderDetailAsync(long customerId, long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

        if (order == null)
            return BaseResponse<OrderOutputResource>.Fail("Order does not exist or does not belong to you.");

        return BaseResponse<OrderOutputResource>.Ok(await BuildOrderOutput(order));
    }

    // =========================================================================
    // CUSTOMER: CANCEL ORDER DIRECTLY ONLY WHEN PENDING
    // =========================================================================
    public async Task<BaseResponse<OrderOutputResource>> CancelOrderAsync(long customerId, long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

        if (order == null)
            return BaseResponse<OrderOutputResource>.Fail("Order does not exist or does not belong to you.");

        if (order.Status != OrderStatus.Pending)
            return BaseResponse<OrderOutputResource>.Fail("Only orders in Pending status can be cancelled.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            // Return stock quantity back to products
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    if (product.Status == ProductStatus.OutOfStock && product.StockQuantity > 0)
                    {
                        product.Status = ProductStatus.Available;
                    }
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BaseResponse<OrderOutputResource>.Ok(await BuildOrderOutput(order), "Order cancelled successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================================
    // ADMIN: GET ALL LIST + DETAIL + COUNTS
    // =========================================================================
    public async Task<BaseResponse<OrderPagedListResource>> GetAdminOrdersAsync(OrderQueryResource query)
    {
        // Calculate counts for each status based on the same filters except the Status filter itself (to show correct counts in each tab)
        var baseCountQuery = _context.Orders.Where(o => o.DeletedAt == null);

        // Apply common filters except the Status filter itself to show correct total counts in each tab
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            baseCountQuery = baseCountQuery.Where(o => o.Id.ToString().Contains(search) ||
                                                       o.ShippingAddress.ToLower().Contains(search));
        }
        if (query.FromDate.HasValue)
            baseCountQuery = baseCountQuery.Where(o => o.CreatedAt >= query.FromDate.Value);
        if (query.ToDate.HasValue)
            baseCountQuery = baseCountQuery.Where(o => o.CreatedAt <= query.ToDate.Value);

        var statusCountsRaw = await baseCountQuery
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var statusCountsDict = Enum.GetValues<OrderStatus>()
            .ToDictionary(e => e.ToString(), _ => 0);

        int allCount = 0;
        foreach (var item in statusCountsRaw)
        {
            if (statusCountsDict.ContainsKey(item.Status))
            {
                statusCountsDict[item.Status] = item.Count;
                allCount += item.Count;
            }
        }
        statusCountsDict["All"] = allCount;

        // Filter the main queryable for pagination and listing based on the original filters including Status filter
        var queryable = _context.Orders.AsNoTracking();
        queryable = ApplyOrderFilters(queryable, query);

        var totalItems = await queryable.CountAsync();
        int page = query.Page > 0 ? query.Page : 1;
        int pageSize = query.PageSize > 0 ? query.PageSize : 12;

        var orders = await queryable
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var outputItems = new List<OrderOutputResource>();
        foreach (var order in orders)
        {
            outputItems.Add(await BuildOrderOutput(order));
        }

        var pagedResult = new OrderPagedListResource
        {
            Items = outputItems,
            TotalItems = totalItems,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            StatusCounts = statusCountsDict
        };

        return BaseResponse<OrderPagedListResource>.Ok(pagedResult, "Get orders for admin successfully.");
    }

    public async Task<BaseResponse<OrderOutputResource>> GetAdminOrderDetailAsync(long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return BaseResponse<OrderOutputResource>.Fail("Cannot find the order.");

        return BaseResponse<OrderOutputResource>.Ok(await BuildOrderOutput(order));
    }

    // =========================================================================
    // ADMIN: STATE MACHINE FOR UPDATING ORDER STATUS
    // =========================================================================
    public async Task<BaseResponse<OrderOutputResource>> UpdateOrderStatusAsync(long orderId, string newStatus)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var targetStatus))
            return BaseResponse<OrderOutputResource>.Fail("Invalid target status.");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return BaseResponse<OrderOutputResource>.Fail("Order not found.");

        // Check the validity of the state machine transition
        bool isValidTransition = order.Status switch
        {
            OrderStatus.Pending => targetStatus == OrderStatus.Confirmed || targetStatus == OrderStatus.Cancelled,
            OrderStatus.Confirmed => targetStatus == OrderStatus.Shipping || targetStatus == OrderStatus.Cancelled,
            OrderStatus.Shipping => targetStatus == OrderStatus.Completed,
            OrderStatus.Completed => false, // Final state, cannot transition to any other state
            OrderStatus.Cancelled => false, // Cancelled is a final state, cannot transition to any other state
            _ => false
        };

        if (!isValidTransition)
            return BaseResponse<OrderOutputResource>.Fail($"Cannot transition from '{order.Status}' to '{targetStatus}'.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // If transitioning to Cancelled from Confirmed, proceed to restock
            if (targetStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        if (product.Status == ProductStatus.OutOfStock && product.StockQuantity > 0)
                        {
                            product.Status = ProductStatus.Available;
                        }
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // If transitioning to Completed, set AmountPaid = TotalPrice
            // (assuming full payment is collected at this point)
            if (targetStatus == OrderStatus.Completed)
            {
                order.AmountPaid = order.TotalPrice;
            }

            order.Status = targetStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BaseResponse<OrderOutputResource>.Ok(await BuildOrderOutput(order), $"Update {targetStatus} successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================================
    // PRIVATE HELPER METHODS
    // =========================================================================
    private static IQueryable<Order> ApplyOrderFilters(IQueryable<Order> queryable, OrderQueryResource query)
    {
        queryable = queryable.Where(o => o.DeletedAt == null);

        if (query.Status.HasValue)
            queryable = queryable.Where(o => o.Status == query.Status.Value);

        if (query.DeliveryMode.HasValue)
            queryable = queryable.Where(o => o.DeliveryMode == query.DeliveryMode.Value);

        if (query.PaymentMethodId.HasValue)
            queryable = queryable.Where(o => o.PaymentMethodId == query.PaymentMethodId.Value);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(o => o.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(o => o.CreatedAt <= query.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            // Tìm kiếm xuyên thấu cột JSONB ShippingAddress của PostgreSQL
            queryable = queryable.Where(o => o.Id.ToString().Contains(search) ||
                                             o.ShippingAddress.ToLower().Contains(search));
        }

        return queryable;
    }

    private Task<OrderOutputResource> BuildOrderOutput(Order order)
    {
        var output = _mapper.Map<OrderOutputResource>(order);

        // Deserialize ShippingAddress JSON back to object for FE display
        if (!string.IsNullOrWhiteSpace(order.ShippingAddress))
        {
            output.ShippingAddress = JsonSerializer.Deserialize<ShippingAddressSnapshot>(order.ShippingAddress);
        }

        // Init AllowedNextStatuses based on the current status of the order to control which buttons should be enabled on FE
        output.AllowedNextStatuses = order.Status switch
        {
            OrderStatus.Pending => new List<string> { OrderStatus.Confirmed.ToString(), OrderStatus.Cancelled.ToString() },
            OrderStatus.Confirmed => new List<string> { OrderStatus.Shipping.ToString(), OrderStatus.Cancelled.ToString() },
            OrderStatus.Shipping => new List<string> { OrderStatus.Completed.ToString() },
            _ => new List<string>() // Completed và Cancelled mảng rỗng (khóa nút bấm)
        };

        // Attach refund flag for FE to show refund status if the order is cancelled but has AmountPaid > 0
        // (Paid but cancelled, awaiting refund)
        output.IsRefundPending = order.Status == OrderStatus.Cancelled && order.AmountPaid > 0;

        return Task.FromResult(output);
    }
}