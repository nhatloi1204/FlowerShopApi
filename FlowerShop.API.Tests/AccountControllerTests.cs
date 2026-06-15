using System.Security.Claims;
using FluentAssertions;
using FlowerShop.API.Controllers;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FlowerShop.API.Tests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<ICustomerAddressService> _mockAddressService;
    private readonly AccountController _controller;
    private const long MockUserId = 123; 

    public AccountControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockAddressService = new Mock<ICustomerAddressService>();

        _controller = new AccountController(_mockCustomerService.Object, _mockAddressService.Object);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, MockUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region Profile Tests

    [Fact]
    public async Task GetProfile_WhenUserExists_ReturnsOkWithData()
    {
        // Arrange (Chuẩn bị dữ liệu giả lập)
        var mockProfile = new CustomerOutputResource { Id = MockUserId, Name = "Developer", Email = "dev@flowershop.com" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Ok(mockProfile, "Success");

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(MockUserId))
            .ReturnsAsync(serviceResponse);

        // Act (Thực hiện hành động call API)
        var result = await _controller.GetProfile();

        // Assert (Check)
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<CustomerOutputResource>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Name.Should().Be("Developer");
    }

    [Fact]
    public async Task GetProfile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var serviceResponse = BaseResponse<CustomerOutputResource>.Fail("Customer not found");
        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(MockUserId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateProfile_WithValidInput_ReturnsOkWithUpdatedData()
    {
        // Arrange
        var input = new CustomerInputResource { Name = "New Name", Phone = "0901234567" };
        var mockUpdatedProfile = new CustomerOutputResource { Id = MockUserId, Name = "New Name", Phone = "0901234567" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Ok(mockUpdatedProfile, "Success");

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(MockUserId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateProfile(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<CustomerOutputResource>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateProfile_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var input = new CustomerInputResource { Name = "Bad Name" };
        var serviceResponse = BaseResponse<CustomerOutputResource>.Fail("Cập nhật thất bại do lỗi hệ thống");

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(MockUserId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateProfile(input);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Address Tests

    [Fact]
    public async Task CreateAddress_WithValidInput_ReturnsOkWithNewAddress()
    {
        // Arrange
        var input = new CustomerAddressInputResource { Name = "Nhà riêng", Address = "99 Tô Hiến Thành, Q10" };
        var mockCreatedAddress = new CustomerAddressOutputResource { Id = 1, Name = "Nhà riêng", Address = "99 Tô Hiến Thành, Q10" };
        var serviceResponse = BaseResponse<CustomerAddressOutputResource>.Ok(mockCreatedAddress, "Success");

        _mockAddressService.Setup(s => s.CreateAddressAsync(MockUserId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.CreateAddress(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseData = okResult.Value.Should().BeOfType<BaseResponse<CustomerAddressOutputResource>>().Subject;
        responseData.Success.Should().BeTrue();
        responseData.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAddress_WhenAuthorized_ReturnsOk()
    {
        // Arrange
        long addressId = 9;
        var input = new CustomerAddressInputResource { Name = "Công ty" };
        var mockUpdatedAddress = new CustomerAddressOutputResource { Id = addressId, Name = "Công ty" };
        var serviceResponse = BaseResponse<CustomerAddressOutputResource>.Ok(mockUpdatedAddress, "Success");

        _mockAddressService.Setup(s => s.UpdateAddressAsync(addressId, MockUserId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateAddress(addressId, input);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteAddress_WhenNotOwner_ReturnsBadRequest()
    {
        // Arrange
        long alienAddressId = 999; // ID địa chỉ của người khác
        var serviceResponse = BaseResponse<bool>.Fail("Không tìm thấy địa chỉ hoặc bạn không có quyền.");

        _mockAddressService.Setup(s => s.DeleteAddressAsync(alienAddressId, MockUserId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.DeleteAddress(alienAddressId);

        // Assert
        // Phải trả về BadRequest vì service chặn không cho xóa trộm địa chỉ người khác
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateAddress_WhenNotOwner_ReturnsBadRequest()
    {
        // Arrange
        long alienAddressId = 999;
        var input = new CustomerAddressInputResource { Name = "Hacker Home" };
        var serviceResponse = BaseResponse<CustomerAddressOutputResource>.Fail("Không có quyền chỉnh sửa");

        _mockAddressService.Setup(s => s.UpdateAddressAsync(alienAddressId, MockUserId, input))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateAddress(alienAddressId, input);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    #endregion
}