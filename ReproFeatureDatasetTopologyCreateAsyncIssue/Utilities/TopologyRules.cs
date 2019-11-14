using System.Collections.Generic;
using System.Linq;

namespace ReproFeatureDatasetTopologyCreateAsyncIssue.Utilities
{
    /// <summary>
    /// Represents all the topology rules available. See
    /// https://pro.arcgis.com/en/pro-app/tool-reference/data-management/add-rule-to-topology.htm,
    /// https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-polygon-features.htm,
    /// https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-polyline-features.htm,
    /// and https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-point-features.htm
    /// for more information.
    /// </summary>
    internal static class TopologyRules
    {
        private static IReadOnlyList<string> _allRules;

        /// <summary>
        /// A read-only list of all topology rules.
        /// </summary>
        public static IReadOnlyList<string> AllRules => _allRules ?? (_allRules = Polygon.AllRules.Concat(Line.AllRules).Concat(Point.AllRules).ToList());

        /// <summary>
        /// Represents all the polygon topology rules available. See
        /// https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-polygon-features.htm,
        /// </summary>
        public static class Polygon
        {
            private static IEnumerable<string> _allRules;

            public static string MustNotHaveGaps => "Must Not Have Gaps (Area)";
            public static string MustNotOverlap => "Must Not Overlap (Area)";
            public static string MustBeCoveredByFeatureClassOf => "Must Be Covered By Feature Class Of (Area-Area)";
            public static string MustCoverEachOther => "Must Cover Each Other (Area-Area)";
            public static string MustBeCoveredBy => "Must Be Covered By (Area-Area)";
            public static string MustNotOverlapWith => "Must Not Overlap With (Area-Area)";
            public static string BoundaryMustBeCoveredBy => "Boundary Must Be Covered By (Area-Line)";
            public static string BoundaryMustBeCoveredByBoundaryOf => "Boundary Must Be Covered By Boundary Of (Area-Area)";
            public static string ContainsPoint => "Contains Point (Area-Point)";
            public static string ContainsOnePoint => "Contains One Point (Area-Point)";

            /// <summary>
            /// All polygon rules.
            /// </summary>
            public static IEnumerable<string> AllRules => _allRules ?? (_allRules = new[] { MustNotHaveGaps, MustNotOverlap, MustBeCoveredByFeatureClassOf, MustCoverEachOther, MustBeCoveredBy, MustNotOverlapWith, BoundaryMustBeCoveredBy, BoundaryMustBeCoveredByBoundaryOf, ContainsPoint, ContainsOnePoint });
        }

        /// <summary>
        /// Represents all the line topology rules available. See
        /// https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-polyline-features.htm,
        /// </summary>
        public static class Line
        {
            private static IEnumerable<string> _allRules;

            public static string MustBeCoveredByBoundaryOf => "Must Be Covered By Boundary Of (Line-Area)";
            public static string MustNotOverlap => "Must Not Overlap (Line)";
            public static string MustNotIntersect => "Must Not Intersect (Line)";
            public static string MustNotHaveDangles => "Must Not Have Dangles (Line)";
            public static string MustNotHavePseudoNodes => "Must Not Have Pseudo-Nodes (Line)";
            public static string MustBeCoveredByFeatureClassOf => "Must Be Covered By Feature Class Of (Line-Line)";
            public static string MustNotOverlapWith => "Must Not Overlap With (Line-Line)";
            public static string MustNotSelfOverlap => "Must Not Self-Overlap (Line)";
            public static string MustNotSelfIntersect => "Must Not Self-Intersect (Line)";
            public static string MustNotIntersectOrTouchInterior => "Must Not Intersect Or Touch Interior (Line)";
            public static string EndpointMustBeCoveredBy => "Endpoint Must Be Covered By (Line-Point)";
            public static string MustBeSinglePart => "Must Be Single Part (Line)";
            public static string MustNotIntersectWith => "Must Not Intersect With (Line-Line)";
            public static string MustNotIntersectOrTouchInteriorWith => "Must Not Intersect or Touch Interior With (Line-Line)";
            public static string MustBeInside => "Must Be Inside (Line-Area)";

            /// <summary>
            /// All line rules.
            /// </summary>
            public static IEnumerable<string> AllRules => _allRules ?? (_allRules = new[] { MustBeCoveredByBoundaryOf, MustNotOverlap, MustNotIntersect, MustNotHaveDangles, MustNotHavePseudoNodes, MustBeCoveredByFeatureClassOf, MustNotOverlapWith, MustNotSelfOverlap, MustNotSelfIntersect, MustNotIntersectOrTouchInterior, EndpointMustBeCoveredBy, MustBeSinglePart, MustNotIntersectWith, MustNotIntersectOrTouchInteriorWith, MustBeInside });
        }

        /// <summary>
        /// Represents all the point topology rules available. See
        /// https://pro.arcgis.com/en/pro-app/help/editing/geodatabase-topology-rules-for-point-features.htm,
        /// </summary>
        public static class Point
        {
            private static IEnumerable<string> _allRules;

            public static string MustBeCoveredByBoundaryOf => "Must Be Covered By Boundary Of (Point-Area)";
            public static string MustBeProperlyInside => "Must Be Properly Inside (Point-Area)";
            public static string MustBeCoveredBy => "Must Be Covered By (Point-Line)";
            public static string MustBeCoveredByEndpointOf => "Must Be Covered By Endpoint Of (Point-Line)";
            public static string MustCoincideWith => "Must Coincide With (Point-Point)";
            public static string MustBeDisjoint => "Must Be Disjoint (Point)";

            /// <summary>
            /// All point rules.
            /// </summary>
            public static IEnumerable<string> AllRules => _allRules ?? (_allRules = new[] { MustBeCoveredByBoundaryOf, MustBeProperlyInside, MustBeCoveredBy, MustBeCoveredByEndpointOf, MustCoincideWith, MustBeDisjoint });
        }
    }
}