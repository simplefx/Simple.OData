using System;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests.Core
{
    public class ODataExpandAssociationTests
    {
        [Fact]
        public void CreateExpandAssociationFromString()
        {
            var association = ODataExpandAssociation.From("Products");
            
            Assert.Equal("Products", association.Name);
        }

        [Fact]
        public void CreateExpandAssociationWithNestedAssociations()
        {
            var association = ODataExpandAssociation.From("Products/Category/Orders");
            
            Assert.Equal("Products", association.Name);
            Assert.Single(association.ExpandAssociations);
            Assert.Equal("Category", association.ExpandAssociations.First().Name);
            Assert.Single(association.ExpandAssociations.First().ExpandAssociations);
            Assert.Equal("Orders", association.ExpandAssociations.First().ExpandAssociations.First().Name);
        }
        
        [Fact]
        public void CreateExpandAssociationFromNullString()
        {
            var association = ODataExpandAssociation.From(null);
            
            Assert.Equal(string.Empty, association.Name);
        }
        
        [Fact]
        public void CreateExpandAssociationFromEmptyString()
        {
            var association = ODataExpandAssociation.From(string.Empty);
            
            Assert.Equal(string.Empty, association.Name);
        }

        [Fact]
        public void CloneProducesNewObjects()
        {
            var association = new ODataExpandAssociation("Products")
            {
                ExpandAssociations = {new ODataExpandAssociation("Category")}
            };

            var clonedAssociation = association.Clone();

            Assert.NotSame(association, clonedAssociation);
            Assert.NotSame(association.ExpandAssociations.First(), clonedAssociation.ExpandAssociations.First());
        }
    }
}