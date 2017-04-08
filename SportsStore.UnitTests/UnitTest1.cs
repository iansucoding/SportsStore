using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.HtmlHelpers;
using SportsStore.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1"},
                new Product{ProductId = 2,Name = "P2"},
                new Product{ProductId = 3,Name = "P3"},
                new Product{ProductId = 4,Name = "P4"},
                new Product{ProductId = 5,Name = "P5"},
            });

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // Act
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            // Assert
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        [TestMethod]
        public void Can_Generate_Page_Link()
        {
            // Arrange
            HtmlHelper myHelper = null;

            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemPerPage = 10
            };

            Func<int, string> pageUrlDelegate = i => "Page" + i;

            // Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            // Assert
            var expected = @"<a class=""btn btn-default"" href=""Page1"">1</a>"
            + @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>"
            + @"<a class=""btn btn-default"" href=""Page3"">3</a>";

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1"},
                new Product{ProductId = 2,Name = "P2"},
                new Product{ProductId = 3,Name = "P3"},
                new Product{ProductId = 4,Name = "P4"},
                new Product{ProductId = 5,Name = "P5"},
            });

            ProductController contoller = new ProductController(mock.Object);
            contoller.PageSize = 3;

            // Act
            ProductsListViewModel result = (ProductsListViewModel)contoller.List(null, 2).Model;

            // Assert
            PagingInfo paginInfo = result.PagingInfo;
            Assert.AreEqual(paginInfo.CurrentPage, 2);
            Assert.AreEqual(paginInfo.ItemPerPage, 3);
            Assert.AreEqual(paginInfo.TotalItems, 5);
            Assert.AreEqual(paginInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Products()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1",Category="Cat1"},
                new Product{ProductId = 2,Name = "P2",Category="Cat2"},
                new Product{ProductId = 3,Name = "P3",Category="Cat1"},
                new Product{ProductId = 4,Name = "P4",Category="Cat2"},
                new Product{ProductId = 5,Name = "P5",Category="Cat3"},
            });

            ProductController contoller = new ProductController(mock.Object);
            contoller.PageSize = 3;

            // Act
            Product[] result = ((ProductsListViewModel)contoller.List("Cat2", 1).Model).Products.ToArray();

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "Cat2");
            Assert.IsTrue(result[1].Name == "P4" && result[0].Category == "Cat2");
        }

        [TestMethod]
        public void Can_Create_Categories()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1",Category="Apples"},
                new Product{ProductId = 2,Name = "P2",Category="Apples"},
                new Product{ProductId = 3,Name = "P3",Category="Plums"},
                new Product{ProductId = 4,Name = "P4",Category="Oranges"},
            });

            NavController target = new NavController(mock.Object);

            // Act
            string[] results = ((IEnumerable<string>)target.Menu().Model).ToArray();

            // Assert
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual("Apples", results[0]);
            Assert.AreEqual("Oranges", results[1]);
            Assert.AreEqual("Plums", results[2]);
        }

        [TestMethod]
        public void Indicates_Selected_Category()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1",Category="Apples"},
                new Product{ProductId = 4,Name = "P4",Category="Oranges"},
            });

            NavController target = new NavController(mock.Object);

            string categoryToSelect = "Apples";

            // Act
            string result = target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            // Assert
            Assert.AreEqual(categoryToSelect, result);
        }

        [TestMethod]
        public void Generate_Category_Specific_Product_Count()
        {
            // Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductId = 1,Name = "P1",Category="Cat1"},
                new Product{ProductId = 2,Name = "P2",Category="Cat2"},
                new Product{ProductId = 3,Name = "P3",Category="Cat1"},
                new Product{ProductId = 4,Name = "P4",Category="Cat2"},
                new Product{ProductId = 5,Name = "P5",Category="Cat3"},
            });

            ProductController target = new ProductController(mock.Object);
            target.PageSize = 3;

            // Act
            int res1 = ((ProductsListViewModel)target.List("Cat1").Model).PagingInfo.TotalItems;
            int res2 = ((ProductsListViewModel)target.List("Cat2").Model).PagingInfo.TotalItems;
            int res3 = ((ProductsListViewModel)target.List("Cat3").Model).PagingInfo.TotalItems;
            int resAll = ((ProductsListViewModel)target.List(null).Model).PagingInfo.TotalItems;

            // Assert
            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
            Assert.AreEqual(resAll, 5);
        }
    }
}