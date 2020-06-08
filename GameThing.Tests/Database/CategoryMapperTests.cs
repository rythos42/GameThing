using System;
using GameThing.Database;
using NUnit.Framework;

namespace GameThing.Tests.Database
{
	[TestFixture]
	public class CategoryMapperTests
	{
		[Test]
		public void LoadCategories_LoadsDataStructure()
		{
			var categoryMapper = (CategoryMapper) CategoryMapper.Instance;
			var jsonString = @"
                [
                    {
                        ""id"": ""categoryId"",
                        ""name"": ""Category Name""
                    }
                ]
            ";
			categoryMapper.LoadCategories(jsonString);

			var returnedCategory = categoryMapper.Get("categoryId");
			Assert.That(returnedCategory.Name, Is.EqualTo("Category Name"));
		}

		[Test]
		public void LoadCategories_ThrowsIfDuplicateIds()
		{
			var categoryMapper = (CategoryMapper) CategoryMapper.Instance;
			var jsonString = @"
                [
                    {
                        ""id"": ""categoryId"",
                        ""name"": ""Category Name""
                    },
                    {
                        ""id"": ""categoryId"",
                        ""name"": ""Category Name""
                    },
                ]
            ";

			Assert.Throws<Exception>(() => categoryMapper.LoadCategories(jsonString), "One of the categories IDs in categories.json is not unique.");
		}
	}
}
