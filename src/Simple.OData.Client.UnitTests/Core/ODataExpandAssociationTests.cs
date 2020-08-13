using Xunit;

namespace Simple.OData.Client.Tests.Core
{
    public class ODataExpandAssociationTests
    {
        [Fact]
        public void SimpleMerge()
        {
            var firstAssociation = new ODataExpandAssociation("Test");
            var secondAssociation = new ODataExpandAssociation("Test");

            var mergedAssociation = ODataExpandAssociation.MergeExpandAssociations(firstAssociation, secondAssociation);
            
            Assert.Equal("Test", mergedAssociation.Name);
            Assert.Empty(mergedAssociation.ExpandAssociations);
        }
        
        [Fact]
        public void MergeTwoLevels()
        {
            var firstAssociation = new ODataExpandAssociation("Test")
            {
                ExpandAssociations = {new ODataExpandAssociation("Test2")}
            };
            var secondAssociation = new ODataExpandAssociation("Test");

            var mergedAssociation = ODataExpandAssociation.MergeExpandAssociations(firstAssociation, secondAssociation);
            
            Assert.Equal("Test", mergedAssociation.Name);
            Assert.Equal("Test2", mergedAssociation.ExpandAssociations[0].Name);
        }
    }
}