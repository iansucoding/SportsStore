using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.Models;
using System.Linq;
using System.Web.Mvc;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // Arrange
            Product p1 = new Product { ProductId = 1, Name = "P1" };
            Product p2 = new Product { ProductId = 2, Name = "P2" };

            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            CartLine[] results = target.Lines.ToArray();

            // Assert
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual(p1, results[0].Product);
            Assert.AreEqual(p2, results[1].Product);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // Arrange
            Product p1 = new Product { ProductId = 1, Name = "P1" };
            Product p2 = new Product { ProductId = 2, Name = "P2" };

            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);

            CartLine[] results = target.Lines.OrderBy(c => c.Product.ProductId).ToArray();

            // Assert
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual(11, results[0].Quantity);
            Assert.AreEqual(1, results[1].Quantity);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            // Arrange
            Product p1 = new Product { ProductId = 1, Name = "P1" };
            Product p2 = new Product { ProductId = 2, Name = "P2" };
            Product p3 = new Product { ProductId = 3, Name = "P3" };

            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            target.RemoveLine(p2);

            // Assert
            Assert.AreEqual(0, target.Lines.Where(c => c.Product == p2).Count());
            Assert.AreEqual(2, target.Lines.Count());
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // Arrange
            Product p1 = new Product { ProductId = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductId = 2, Name = "P2", Price = 50M };

            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);

            decimal result = target.ComputeTotalValue();

            // Assert
            Assert.AreEqual(450M, result);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            // Arrange
            Product p1 = new Product { ProductId = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductId = 2, Name = "P2", Price = 50M };

            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            target.Clear();

            // Assert
            Assert.AreEqual(0, target.Lines.Count());
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product{ProductId=1,Name="P1",Category = "Apples"},
            }.AsQueryable());

            Cart cart = new Cart();

            CartController target = new CartController(mock.Object, null);

            // Act
            target.AddToCart(cart, 1, null);

            // Asert
            Assert.AreEqual(1, cart.Lines.Count(), 1);
            Assert.AreEqual(1, cart.Lines.ToArray()[0].Product.ProductId);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product{ProductId=1,Name="P1",Category = "Apples"},
            }.AsQueryable());

            Cart cart = new Cart();

            CartController target = new CartController(mock.Object, null);

            // Act
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            // Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("myUrl", result.RouteValues["returnUrl"]);
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // Arrange
            Cart cart = new Cart();

            CartController target = new CartController(null, null);

            // Act
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // Assert
            Assert.AreSame(cart, result.Cart);
            Assert.AreSame("myUrl", result.ReturnUrl);
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // Arrage
            Mock<IOderProcessor> mock = new Mock<IOderProcessor>();

            Cart cart = new Cart();

            ShippingDetails shippingDetails = new ShippingDetails();

            CartController target = new CartController(null, mock.Object);

            // Act
            ViewResult result = target.Checkout(cart, shippingDetails);

            // Assert
            // 不會調用 IOderProcessor 模仿實現的 ProcessOrder 方法
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never);
            // 返回默認的 View
            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetail()
        {
            // Arrage
            Mock<IOderProcessor> mock = new Mock<IOderProcessor>();

            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            CartController target = new CartController(null, mock.Object);

            target.ModelState.AddModelError("error", "error");

            // Act
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never);

            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }
        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            // Arrange
            Mock<IOderProcessor> mock = new Mock<IOderProcessor>();

            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            CartController target = new CartController(null, mock.Object);

            // Act
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert
            // 會調用 IOderProcessor 模仿實現的 ProcessOrder 方法
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());

            Assert.AreEqual("Completed", result.ViewName);

            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
            
        }
    }
}