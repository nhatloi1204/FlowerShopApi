using FluentAssertions;
using FlowerShop.API.Controllers;
using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FlowerShop.API.Tests.Controllers;

public class AdminCustomerControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly AdminCustomerController _controller;

    public AdminCustomerControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _controller = new AdminCustomerController(_mockCustomerService.Object);
    }


    #region Basic CRUD Tests
    [Fact]
    public async Task GetAll_WithValidQuery_ReturnsOkWithPagedList()
    {
        // Arrange
        var query = new CustomerQueryResource { Page = 1, PageSize = 10, Search = "An" };
        var mockPagedList = new PagedList<CustomerOutputResource>
        {
            Items = new List<CustomerOutputResource>
            {
                new() { Id = 1, Name = "Trần Văn An", Email = "an@gmail.com" }
            },
            TotalItems = 1,
            CurrentPage = 1,
            TotalPages = 1
        };
        var serviceResponse = BaseResponse<PagedList<CustomerOutputResource>>.Ok(mockPagedList, "Success");

        _mockCustomerService.Setup(s => s.GetCustomersAsync(query))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<PagedList<CustomerOutputResource>>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Items.Should().HaveCount(1);
        responseData.Data.Items.First().Name.Should().Be("Trần Văn An");
    }

    [Fact]
    public async Task GetById_WhenAdminLooksUpExistingCustomer_ReturnsOk()
    {
        // Arrange
        long targetCustomerId = 55;
        var mockCustomer = new CustomerOutputResource { Id = targetCustomerId, Name = "Khách VIP" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Ok(mockCustomer, "Success");

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(targetCustomerId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetById(targetCustomerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<CustomerOutputResource>>().Subject;
        responseData.Data.Id.Should().Be(targetCustomerId);
    }

    [Fact]
    public async Task GetById_WhenCustomerDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        long targetCustomerId = 404;
        var serviceResponse = BaseResponse<CustomerOutputResource>.Fail("Customer not found");

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(targetCustomerId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetById(targetCustomerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenCustomerExists_ReturnsOk()
    {
        // Arrange
        long targetCustomerId = 99;
        var serviceResponse = BaseResponse<bool>.Ok(true, "Deleted successfully");

        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(targetCustomerId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Delete(targetCustomerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenCustomerNotFound_ReturnsBadRequest()
    {
        // Arrange
        long targetCustomerId = 404;
        var serviceResponse = BaseResponse<bool>.Fail("Customer not found");

        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(targetCustomerId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Delete(targetCustomerId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    #endregion

    #region Search Tests

    [Fact]
    public async Task GetAll_WhenSearchReturnsNoResults_ReturnsOkWithEmptyList()
    {
        // Arrange: Tìm kiếm một cái tên không tồn tại (Ví dụ: "Hacker")
        var query = new CustomerQueryResource { Page = 1, PageSize = 10, Search = "Hacker123" };
        var mockEmptyPagedList = new PagedList<CustomerOutputResource>
        {
            Items = new List<CustomerOutputResource>(), // Trống rỗng
            TotalItems = 0,
            CurrentPage = 1,
            TotalPages = 0
        };
        var serviceResponse = BaseResponse<PagedList<CustomerOutputResource>>.Ok(mockEmptyPagedList, "No customers found");

        _mockCustomerService.Setup(s => s.GetCustomersAsync(query))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetAll(query);

        // Assert: Hệ thống vẫn phải trả về 200 OK kèm mảng rỗng chứ không được crash hay trả về Null
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<PagedList<CustomerOutputResource>>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Items.Should().BeEmpty();
        responseData.Data.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WhenQueryContainsEdgeCasePageNumbers_ReturnsOk()
    {
        // Arrange: Giả lập trường hợp Front-End truyền tham số không tồn tại (Trang 99999 không tồn tại)
        var query = new CustomerQueryResource { Page = 99999, PageSize = 50 };
        var mockEmptyPagedList = new PagedList<CustomerOutputResource>
        {
            Items = new List<CustomerOutputResource>(),
            TotalItems = 10, // Tổng hệ thống có 10 items, nhưng phân trang ở trang quá lớn
            CurrentPage = 99999,
            TotalPages = 1
        };
        var serviceResponse = BaseResponse<PagedList<CustomerOutputResource>>.Ok(mockEmptyPagedList, "Page out of bounds");

        _mockCustomerService.Setup(s => s.GetCustomersAsync(query))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<PagedList<CustomerOutputResource>>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Items.Should().BeEmpty();
    }
    #endregion

    #region Admin Update Tests

    [Fact]
    public async Task Update_WithValidInput_ReturnsOkWithUpdatedCustomer()
    {
        // Arrange
        long targetCustomerId = 1;
        var input = new CustomerInputResource { Name = "Tên Sửa Bởi Admin", Phone = "0988888888" };
        var mockUpdatedCustomer = new CustomerOutputResource { Id = targetCustomerId, Name = "Tên Sửa Bởi Admin", Phone = "0988888888" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Ok(mockUpdatedCustomer, "Updated successfully");

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(targetCustomerId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Update(targetCustomerId, input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<CustomerOutputResource>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Name.Should().Be("Tên Sửa Bởi Admin");
    }

    [Fact]
    public async Task Update_WhenCustomerNotFound_ReturnsBadRequest()
    {
        // Arrange
        long invalidCustomerId = 404;
        var input = new CustomerInputResource { Name = "Ghost User" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Fail("Customer not found");

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(invalidCustomerId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Update(invalidCustomerId, input);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenServiceFailsOnDatabaseException_ReturnsBadRequest()
    {
        // Arrange
        long targetCustomerId = 1;
        var input = new CustomerInputResource { Name = "Trùng Email Chẳng Hạn" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Fail("Lỗi hệ thống hoặc xung đột dữ liệu.");

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(targetCustomerId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Update(targetCustomerId, input);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var responseData = badRequestResult.Value.Should().BeOfType<BaseResponse<CustomerOutputResource>>().Subject;
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("xung đột dữ liệu");
    }

    #endregion
}