using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public class ODataExpandAssociation
    {
        public ODataExpandAssociation(string name)
        {
            Name = name;
        }
        
        public string Name { get; }
        
        public List<ODataExpandAssociation> ExpandAssociations { get; } = new List<ODataExpandAssociation>();
        
        public List<ODataOrderByColumn> OrderByColumns { get; }  = new List<ODataOrderByColumn>();
        
        public ODataExpression FilterExpression { get; set; }

        public static ODataExpandAssociation MergeExpandAssociations(ODataExpandAssociation expandAssociation, string path)
        {
            return MergeExpandAssociations(expandAssociation, From(path));
        }
        
        public static ODataExpandAssociation MergeExpandAssociations(ODataExpandAssociation first, ODataExpandAssociation second)
        {
            var result = first.Clone();
            if (result.Name != second.Name && result.Name != "*") return result;

            var expandAssociations = new List<ODataExpandAssociation>(result.ExpandAssociations);
            result.ExpandAssociations.Clear();
            foreach (var association in expandAssociations)
            {
                result.ExpandAssociations.Add(second.ExpandAssociations.Aggregate(association, MergeExpandAssociations));
            }
            foreach (var association in second.ExpandAssociations)
            {
                if (result.ExpandAssociations.All(a => a.Name != association.Name))
                {
                    result.ExpandAssociations.Add(association.Clone());
                }
            }
            
            return result;
        }

        public static ODataExpandAssociation From(string association)
        {
            if (string.IsNullOrEmpty(association))
                return new ODataExpandAssociation(string.Empty);
            
            var items = association.Split('/');
            var expandAssociation = new ODataExpandAssociation(items.First());
            var currentAssociation = expandAssociation;
            foreach (var item in items.Skip(1))
            {
                currentAssociation.ExpandAssociations.Add(new ODataExpandAssociation(item));
                currentAssociation = currentAssociation.ExpandAssociations.First();
            }

            return expandAssociation;
        }

        public ODataExpandAssociation Clone()
        {
            var clone = new ODataExpandAssociation(Name);
            clone.ExpandAssociations.AddRange(ExpandAssociations.Select(a => a.Clone()));
            clone.FilterExpression = FilterExpression;
            clone.OrderByColumns.AddRange(OrderByColumns);
            return clone;
        }
    }
}