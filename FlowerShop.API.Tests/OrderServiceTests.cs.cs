using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Enums;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Concrete;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace FlowerShop.API.Tests;

public class OrderServiceTests
{
    private readonly IMapper _mapper;

    public OrderServiceTests()
    {
        // Khởi tạo Mapper thật giống hệt cấu hình MappingProfile trong dự án của mày
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = config.CreateMapper();
    }

    // Helper tạo DbContext chạy ngầm trong RAM sạch sẽ cho mỗi Test Case
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    // =========================================================================
    // CUSTOMER: CREATE ORDER TESTS
    // =========================================================================

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFail_WhenItemsIsEmpty()
    {
        using var context = CreateInMemoryDbContext();
        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource { Items = new List<OrderItemInputResource>() };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order must contain at least one item.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFail_WhenAddressNotFound()
    {
        using var context = CreateInMemoryDbContext();
        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 999, // ID ma không tồn tại
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 1 } }
        };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Customer address not found.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFail_WhenPaymentMethodDoesNotExist()
    {
        using var context = CreateInMemoryDbContext();
        var provinceMock = new Province { Id = 1, Name = "Hồ Chí Minh" };
        var wardMock = new Ward { Id = 1, Name = "Phường Tân Phong" };

        var address = new CustomerAddress
        {
            Id = 1,
            CustomerId = 1,
            Name = "Địa chỉ mặc định", // 🎯 FIX: Bổ sung thuộc tính Name bắt buộc của CustomerAddress
            Address = "123 Lý Thường Kiệt",
            ReceiveCustomerName = "Lợi",
            ReceiveCustomerPhone = "0901234567",
            Province = provinceMock,
            Ward = wardMock
        };

        context.Provinces.Add(provinceMock);
        context.Wards.Add(wardMock);
        context.CustomerAddresses.Add(address);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 1,
            PaymentMethodId = 999, // Phương thức fake
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 1 } }
        };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payment method does not exist.");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFail_WhenProductIsDiscontinued()
    {
        using var context = CreateInMemoryDbContext();
        var provinceMock = new Province { Id = 1, Name = "Hồ Chí Minh" };
        var wardMock = new Ward { Id = 1, Name = "Phường Tân Phong" };
        context.Provinces.Add(provinceMock);
        context.Wards.Add(wardMock);

        context.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 1,
            CustomerId = 1,
            Name = "Địa chỉ mặc định", // 🎯 FIX: Bổ sung Name ở đây
            Address = "123 Lý Thường Kiệt",
            ReceiveCustomerName = "Lợi",
            ReceiveCustomerPhone = "0901234567",
            Province = provinceMock,
            Ward = wardMock
        });
        context.PaymentMethods.Add(new PaymentMethod { Id = 1, Name = "COD" });
        context.Products.Add(new Product { Id = 1, Name = "Hoa Hồng", Status = ProductStatus.Discontinued, StockQuantity = 10 });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 1,
            PaymentMethodId = 1,
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 2 } }
        };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("does not exist or has been discontinued");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFail_WhenStockIsInsufficient()
    {
        using var context = CreateInMemoryDbContext();

        var provinceMock = new Province { Id = 1, Name = "Hồ Chí Minh" };
        var wardMock = new Ward { Id = 1, Name = "Phường Tân Phong" };

        context.Provinces.Add(provinceMock);
        context.Wards.Add(wardMock);

        context.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 1,
            CustomerId = 1,
            Name = "Địa chỉ mặc định", // 🎯 FIX: Bổ sung Name ở đây
            Address = "123 Lý Thường Kiệt",
            ReceiveCustomerName = "Lợi",
            ReceiveCustomerPhone = "0901234567",
            Province = provinceMock,
            Ward = wardMock
        });
        context.PaymentMethods.Add(new PaymentMethod { Id = 1, Name = "COD" });
        context.Products.Add(new Product { Id = 1, Name = "Hoa Lan", Status = ProductStatus.Available, StockQuantity = 2 });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 1,
            PaymentMethodId = 1,
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 5 } }
        };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("is out of stock or insufficient quantity");

        var product = await context.Products.FindAsync(1L);
        product!.StockQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldSuccess_AndDeductStock_AndSaveSnapshots()
    {
        using var context = CreateInMemoryDbContext();
        var provinceMock = new Province { Id = 1, Name = "Hồ Chí Minh" };
        var wardMock = new Ward { Id = 1, Name = "Phường Tân Phong" };

        var address = new CustomerAddress
        {
            Id = 1,
            CustomerId = 1,
            Name = "Địa chỉ mặc định", // 🎯 FIX: Bổ sung Name ở đây
            Address = "123 Lý Thường Kiệt",
            ReceiveCustomerName = "Lợi",
            ReceiveCustomerPhone = "0901234567",
            Province = provinceMock,
            Ward = wardMock
        };

        context.Provinces.Add(provinceMock);
        context.Wards.Add(wardMock);
        context.CustomerAddresses.Add(address);
        context.PaymentMethods.Add(new PaymentMethod { Id = 1, Name = "MoMo" });
        context.Products.Add(new Product { Id = 1, Name = "Hoa Ly", Price = 100000, Status = ProductStatus.Available, StockQuantity = 5 });
        context.Medias.Add(new Media { Id = 1, ModelType = "Product", ModelId = 1, FileName = "hoa-ly.jpg", OrderColumn = 1 });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 1,
            PaymentMethodId = 1,
            ReceiveDate = DateTime.Today,
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 5 } }
        };

        var result = await orderService.CreateOrderAsync(customerId: 1, input);

        result.Success.Should().BeTrue();
        result.Data!.TotalPrice.Should().Be(500000);
        result.Data.AllowedNextStatuses.Should().Contain(new[] { "Confirmed", "Cancelled" });

        var product = await context.Products.FindAsync(1L);
        product!.StockQuantity.Should().Be(0);
        product.Status.Should().Be(ProductStatus.OutOfStock);

        var orderInDb = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == result.Data.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.OrderItems.First().ProductImage.Should().Be("hoa-ly.jpg");

        // 🎯 FIX CHÍ MẠNG: Deserialize chuỗi JSONB ra để check chuẩn chỉ cấu trúc
        var shippingAddressObj = JsonSerializer.Deserialize<Dictionary<string, string>>(orderInDb.ShippingAddress);
        shippingAddressObj.Should().NotBeNull();
        shippingAddressObj!["FullAddress"].Should().Be("123 Lý Thường Kiệt");
        shippingAddressObj["ReceiveCustomerName"].Should().Be("Lợi");
        shippingAddressObj["ProvinceName"].Should().Be("Hồ Chí Minh");
    }

    // =========================================================================
    // CUSTOMER: CANCEL ORDER TESTS
    // =========================================================================

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnFail_WhenOrderNotFoundOrWrongCustomer()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Pending });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        var result = await orderService.CancelOrderAsync(customerId: 2, orderId: 1);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order does not exist or does not belong to you.");
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnFail_WhenOrderStatusIsNotPending()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Confirmed });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.CancelOrderAsync(customerId: 1, orderId: 1);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Only orders in Pending status can be cancelled.");
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldSuccess_AndRestockProducts()
    {
        using var context = CreateInMemoryDbContext();
        var product = new Product { Id = 1, Name = "Hoa Huong Duong", StockQuantity = 0, Status = ProductStatus.OutOfStock };
        var order = new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Pending };
        order.OrderItems.Add(new OrderItem { ProductId = 1, Quantity = 3, UnitPrice = 20000, ProductName = "Hoa Huong Duong" });

        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.CancelOrderAsync(customerId: 1, orderId: 1);

        result.Success.Should().BeTrue();

        var updatedProduct = await context.Products.FindAsync(1L);
        updatedProduct!.StockQuantity.Should().Be(3);
        updatedProduct.Status.Should().Be(ProductStatus.Available);

        var updatedOrder = await context.Orders.FindAsync(1L);
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }

    // =========================================================================
    // ADMIN: UPDATE ORDER STATUS (STATE MACHINE) TESTS
    // =========================================================================

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFail_WhenInvalidStatusString()
    {
        using var context = CreateInMemoryDbContext();
        var orderService = new OrderService(context, _mapper);

        var result = await orderService.UpdateOrderStatusAsync(orderId: 1, newStatus: "StatusFakeLamNe");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid target status.");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFail_WhenTransitionIsInvalid()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, Status = OrderStatus.Pending });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        var result = await orderService.UpdateOrderStatusAsync(orderId: 1, newStatus: "Completed");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot transition from 'Pending' to 'Completed'.");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldSuccess_AndSetAmountPaid_WhenTransitionToCompleted()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, Status = OrderStatus.Shipping, TotalPrice = 350000, AmountPaid = 0 });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.UpdateOrderStatusAsync(orderId: 1, newStatus: "Completed");

        result.Success.Should().BeTrue();

        var order = await context.Orders.FindAsync(1L);
        order!.Status.Should().Be(OrderStatus.Completed);
        order.AmountPaid.Should().Be(350000);
    }

    // =========================================================================
    // PRIVATE METHOD LOGIC: REFUND FLAG TESTS
    // =========================================================================

    [Fact]
    public async Task BuildOrderOutput_ShouldSetIsRefundPendingTrue_WhenOrderIsCancelledAndPaid()
    {
        using var context = CreateInMemoryDbContext();
        var order = new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Cancelled, AmountPaid = 150000 };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.GetCustomerOrderDetailAsync(customerId: 1, orderId: 1);

        result.Success.Should().BeTrue();
        result.Data!.IsRefundPending.Should().BeTrue();
    }

    // =========================================================================
    // CODE BỔ SUNG: 10 TEST CASES CÒN THIẾU THEO MA TRẬN LOGIC
    // =========================================================================

    // KỊCH BẢN 18: Giả lập lỗi hệ thống bất ngờ để test Rollback Transaction
    [Fact]
    public async Task CreateOrderAsync_ShouldRollbackTransaction_WhenAnyExceptionOccurs()
    {
        // Khởi tạo databaseName duy nhất cho kịch bản này
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        // Sử dụng DbContext có bẫy lỗi để chạy dữ liệu mồi
        using var context = new FaultyAppDbContext(options);

        var provinceMock = new Province { Id = 1, Name = "Hồ Chí Minh" };
        var wardMock = new Ward { Id = 1, Name = "Phường Tân Phong" };
        context.Provinces.Add(provinceMock);
        context.Wards.Add(wardMock);

        context.CustomerAddresses.Add(new CustomerAddress
        {
            Id = 1,
            CustomerId = 1,
            Name = "Địa chỉ văn phòng",
            Address = "123 Lý Thường Kiệt",
            ReceiveCustomerName = "Lợi",
            ReceiveCustomerPhone = "0901234567",
            Province = provinceMock,
            Ward = wardMock
        });
        context.PaymentMethods.Add(new PaymentMethod { Id = 1, Name = "COD" });
        context.Products.Add(new Product { Id = 1, Name = "Hoa Trúc", Status = ProductStatus.Available, StockQuantity = 10 });

        // Lưu data mồi thành công (Vì IsReadyToThrow mặc định là false)
        await context.SaveChangesAsync();

        // Kích hoạt bẫy lỗi ngay trước khi gọi Service xử lý đơn hàng
        context.IsReadyToThrow = true;

        var orderService = new OrderService(context, _mapper);
        var input = new OrderCreateInputResource
        {
            ReceiveAddressId = 1,
            PaymentMethodId = 1,
            Items = new List<OrderItemInputResource> { new() { ProductId = 1, Quantity = 2 } }
        };

        // Khai báo hành động gọi Service tạo đơn
        Func<Task> act = async () => await orderService.CreateOrderAsync(customerId: 1, input);

        // Đảm bảo hệ thống ném ra đúng Exception dính bẫy khi gọi SaveChanges
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Postgres Database Connection Timeout! Transactions Rollbacked.");

        using var verifyContext = new AppDbContext(options);
        var productInDb = await verifyContext.Products.FindAsync(1L);

        // Vì tiến trình SaveChanges cũ đã tạch, dữ liệu kho thực tế không bao giờ bị ghi đè -> Chắc chắn bằng 10!
        productInDb!.StockQuantity.Should().Be(10);
    }

    // KỊCH BẢN 11: Admin - Truyền OrderId không tồn tại khi cập nhật trạng thái đơn
    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFail_WhenOrderNotFound()
    {
        using var context = CreateInMemoryDbContext();
        var orderService = new OrderService(context, _mapper);

        var result = await orderService.UpdateOrderStatusAsync(orderId: 9999, newStatus: "Confirmed");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found.");
    }

    // KỊCH BẢN 12: Admin - Duyệt tuần tự luồng chuẩn (Pending -> Confirmed -> Shipping)
    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldSuccess_AndAdvanceStatus_InNormalFlow()
    {
        using var context = CreateInMemoryDbContext();
        var order = new Order { Id = 10, Status = OrderStatus.Pending };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        // Bước 1: Lên Confirmed
        var result1 = await orderService.UpdateOrderStatusAsync(orderId: 10, newStatus: "Confirmed");
        result1.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);

        // Bước 2: Lên Shipping
        var result2 = await orderService.UpdateOrderStatusAsync(orderId: 10, newStatus: "Shipping");
        result2.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipping);
    }

    // KỊCH BẢN 14: Admin chủ động hủy đơn (đơn đang ở trạng thái Pending hoặc Confirmed) -> Phải hoàn kho giống Khách hủy
    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldSuccess_AndRestock_WhenAdminCancelled()
    {
        using var context = CreateInMemoryDbContext();
        var product = new Product { Id = 5, Name = "Hoa Cẩm Chướng", StockQuantity = 2, Status = ProductStatus.Available };
        var order = new Order { Id = 20, Status = OrderStatus.Confirmed };
        order.OrderItems.Add(new OrderItem { ProductId = 5, Quantity = 3, UnitPrice = 50000, ProductName = "Hoa Cẩm Chướng" });

        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.UpdateOrderStatusAsync(orderId: 20, newStatus: "Cancelled");

        result.Success.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        // Kiểm tra logic hoàn kho: 2 + 3 = 5 sản phẩm
        var updatedProduct = await context.Products.FindAsync(5L);
        updatedProduct!.StockQuantity.Should().Be(5);
    }

    // KỊCH BẢN 16: Lọc dữ liệu - Tuyệt đối không hiển thị đơn hàng đã bị xóa mềm (DeletedAt != null)
    [Fact]
    public async Task ApplyOrderFilters_ShouldIgnoreSoftDeletedOrders()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Pending, DeletedAt = null });
        context.Orders.Add(new Order { Id = 2, CustomerId = 1, Status = OrderStatus.Completed, DeletedAt = DateTime.UtcNow }); // Bị xóa mềm
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.GetAdminOrdersAsync(new OrderQueryResource { Page = 1, PageSize = 10 });

        // Kết quả chỉ được lấy đơn ID 1, bỏ qua đơn ID 2
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Id.Should().Be(1);
    }

    // KỊCH BẢN 17 (Bổ sung cờ false): Đơn bị hủy nhưng chưa thanh toán (AmountPaid == 0) -> IsRefundPending phải bằng false
    [Fact]
    public async Task BuildOrderOutput_ShouldSetIsRefundPendingFalse_WhenOrderIsCancelledButNotPaid()
    {
        using var context = CreateInMemoryDbContext();
        var order = new Order { Id = 50, CustomerId = 1, Status = OrderStatus.Cancelled, AmountPaid = 0 };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var result = await orderService.GetCustomerOrderDetailAsync(customerId: 1, orderId: 50);

        result.Success.Should().BeTrue();
        result.Data!.IsRefundPending.Should().BeFalse();
    }

    // KỊCH BẢN 19: Bộ lọc nâng cao - Tìm kiếm theo khoảng ngày, phương thức giao và phương thức thanh toán
    [Fact]
    public async Task ApplyOrderFilters_ShouldFilterByDates_AndDeliveryMode_AndPaymentMethod()
    {
        using var context = CreateInMemoryDbContext();
        var baseTime = new DateTime(2026, 5, 20);

        // Đã cập nhật đúng Enum: DeliveryMode.Instant và DeliveryMode.Scheduled
        context.Orders.Add(new Order { Id = 101, CreatedAt = baseTime, DeliveryMode = DeliveryMode.Instant, PaymentMethodId = 1 });
        context.Orders.Add(new Order { Id = 102, CreatedAt = baseTime.AddDays(5), DeliveryMode = DeliveryMode.Scheduled, PaymentMethodId = 1 }); // Sai khoảng ngày, sai Mode
        context.Orders.Add(new Order { Id = 103, CreatedAt = baseTime, DeliveryMode = DeliveryMode.Instant, PaymentMethodId = 2 }); // Sai PaymentMethodId
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);
        var query = new OrderQueryResource
        {
            FromDate = baseTime.AddDays(-1),
            ToDate = baseTime.AddDays(2),
            DeliveryMode = DeliveryMode.Instant, // Truyền chuỗi "Instant" khớp với Enum tên DeliveryMode.Instant
            PaymentMethodId = 1,
            Page = 1,
            PageSize = 10
        };

        var result = await orderService.GetAdminOrdersAsync(query);
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Id.Should().Be(101);
    }

    // KỊCH BẢN 20: Tìm kiếm xuyên thấu dữ liệu JSONB ShippingAddress lưu trữ dưới dạng Text trong Database
    [Fact]
    public async Task ApplyOrderFilters_ShouldSearchThroughPostgresJsonbShippingAddress()
    {
        using var context = CreateInMemoryDbContext();

        var jsonMatch = "{\"FullAddress\":\"123 Ly Thuong Kiet\",\"ReceiveCustomerName\":\"Anh Loi TDTU\",\"ProvinceName\":\"Ho Chi Minh\"}";
        var jsonMismatch = "{\"FullAddress\":\"456 Tran Hung Dao\",\"ReceiveCustomerName\":\"Nguyen Van A\",\"ProvinceName\":\"Ha Noi\"}";

        context.Orders.Add(new Order { Id = 1, ShippingAddress = jsonMatch });
        context.Orders.Add(new Order { Id = 2, ShippingAddress = jsonMismatch });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        // Tìm kiếm với từ khóa "Loi"
        var query = new OrderQueryResource { Search = "Loi", Page = 1, PageSize = 10 };
        var result = await orderService.GetAdminOrdersAsync(query);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items.First().Id.Should().Be(1);
    }

    // KỊCH BẢN 21: Đếm tổng số lượng đơn theo từng trạng thái Tab (StatusCounts) bất chấp bộ lọc Tab hiện tại
    [Fact]
    public async Task GetAdminOrdersAsync_ShouldCalculateStatusCountsCorrectly_RegardlessOfStatusFilter()
    {
        using var context = CreateInMemoryDbContext();
        context.Orders.Add(new Order { Id = 1, Status = OrderStatus.Pending });
        context.Orders.Add(new Order { Id = 2, Status = OrderStatus.Pending });
        context.Orders.Add(new Order { Id = 3, Status = OrderStatus.Confirmed });
        context.Orders.Add(new Order { Id = 4, Status = OrderStatus.Shipping });
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        // Admin đang bấm xem Tab "Pending" -> Hệ thống filter trả ra 2 đơn, nhưng con số thống kê tổng của các Tab khác vẫn phải đầy đủ
        var query = new OrderQueryResource { Status = OrderStatus.Pending, Page = 1, PageSize = 10 };
        var result = await orderService.GetAdminOrdersAsync(query);

        result.Data!.Items.Should().HaveCount(2); // Danh sách trả về chỉ có 2 đơn Pending

        // Khớp kiểm tra cấu trúc dictionary bộ đếm số lượng trên giao diện (UI Tabs Counter)
        result.Data.StatusCounts["Pending"].Should().Be(2);
        result.Data.StatusCounts["Confirmed"].Should().Be(1);
        result.Data.StatusCounts["Shipping"].Should().Be(1);
        result.Data.StatusCounts["All"].Should().Be(4);
    }

    // KỊCH BẢN 22: Phân trang toán học Math.Ceiling (VD: 13 đơn hàng, PageSize = 12 => Phải tính ra 2 trang)
    [Fact]
    public async Task GetCustomerOrderHistoryAsync_ShouldReturnCorrectPaginationStructure()
    {
        using var context = CreateInMemoryDbContext();
        // Vòng lặp tạo nhanh 13 đơn hàng mẫu cho CustomerId = 10
        for (int i = 1; i <= 13; i++)
        {
            context.Orders.Add(new Order { Id = i, CustomerId = 10, Status = OrderStatus.Pending });
        }
        await context.SaveChangesAsync();

        var orderService = new OrderService(context, _mapper);

        // Gọi xem trang số 1 với cấu hình kích thước trang là 12
        var historyQuery = new OrderQueryResource { Page = 1, PageSize = 12 };
        var result = await orderService.GetCustomerOrderHistoryAsync(10, historyQuery);

        result.Data!.Items.Should().HaveCount(12); // Trang 1 lấy đủ 12 phần tử đầu
        result.Data.TotalItems.Should().Be(13);    // Tổng số lượng bản ghi là 13
        result.Data.TotalPages.Should().Be(2);     // Math.Ceiling(13.0 / 12.0) = 2 trang chuẩn chỉnh
    }
}

// Helper class phục vụ riêng cho Kịch bản 18 (Giả lập một DbContext luôn ném lỗi hệ thống khi lưu dữ liệu)
public class FaultyAppDbContext : AppDbContext
{
    // Cờ kiểm soát: false = cho phép lưu bình thường, true = kích nổ lỗi
    public bool IsReadyToThrow { get; set; } = false;

    public FaultyAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (IsReadyToThrow)
        {
            throw new Exception("Postgres Database Connection Timeout! Transactions Rollbacked.");
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}