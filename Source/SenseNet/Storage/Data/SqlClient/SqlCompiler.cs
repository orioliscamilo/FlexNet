//using System;
//using SenseNet.ContentRepository.Storage.Search;
//using System.Collections.Generic;
//using System.Text;
//using SenseNet.ContentRepository.Storage.Schema;
//using System.Globalization;
//using SenseNet.ContentRepository.Storage.Security;
//using System.Data.SqlClient;
//using System.Linq;
//using SenseNet.ContentRepository.Storage.Search.Internal;

//namespace SenseNet.ContentRepository.Storage.Data.SqlClient
//{
//    internal class SqlCompiler : INodeQueryCompiler
//    {
//        /*
//--**************************************************** MaxLevel: 0 ************************************

//SELECT DISTINCT ROOT0.NodeId, ROOT0.Name
//FROM

//---- Level1

//SysSearchWithFlatsView AS ROOT0						-- root table
//WHERE ROOT0.Name = 'Gyeby'

//--**************************************************** MaxLevel: 1 ************************************

//SELECT DISTINCT ROOT0.NodeId, ROOT0.Name
//FROM

//---- Level1

//SysSearchWithFlatsView AS ROOT0						-- root table

//LEFT OUTER JOIN
//SysSearchWithFlatsView AS ATTR0						-- attribute table branch
//ON 
//    ATTR0.NodeId = ROOT0.ParentNodeId OR 
//    ATTR0.NodeId = ROOT0.CreatedById OR 
//    ATTR0.NodeId = ROOT0.ModifiedById OR 
//    ATTR0.NodeId = ROOT0.LockedById 

//LEFT OUTER JOIN
//ReferenceProperties AS SLOT0			-- SLOT0 table branch
//    LEFT OUTER JOIN
//    SysSearchWithFlatsView AS REF0					-- Referenced by slot
//    ON REF0.NodeId = SLOT0.ReferredNodeId 
//ON ROOT0.VersionId = SLOT0.VersionId

//WHERE
//    ATTR0.NodeId = 1
//    OR
//    (SLOT0.PropertyTypeId = 3 AND REF0.Name = 'Gyeby')

//--**************************************************** MaxLevel: 2 ************************************

//SELECT DISTINCT ROOT0.NodeId, ROOT0.Name
//FROM

//---- Level1

//SysSearchWithFlatsView AS ROOT0						-- root table

//LEFT OUTER JOIN
//SysSearchWithFlatsView AS ATTR0						-- attribute table branch
//ON 
//    ATTR0.NodeId = ROOT0.ParentNodeId OR 
//    ATTR0.NodeId = ROOT0.CreatedById OR 
//    ATTR0.NodeId = ROOT0.ModifiedById OR 
//    ATTR0.NodeId = ROOT0.LockedById 

//LEFT OUTER JOIN
//ReferenceProperties AS SLOT0			-- SLOT0 table branch
//    LEFT OUTER JOIN
//    SysSearchWithFlatsView AS REF0					-- Referenced by slot
//    ON REF0.NodeId = SLOT0.ReferredNodeId 
//ON ROOT0.VersionId = SLOT0.VersionId

//---- Level2

//LEFT OUTER JOIN 
//SysSearchWithFlatsView AS ROOT1 					-- Connector table (Level1-2)
//ON REF0.NodeId = ROOT1.NodeId OR ATTR0.NodeId = ROOT1.NodeId

//LEFT OUTER JOIN
//SysSearchWithFlatsView AS ATTR1						-- attribute table branch (Level2)
//ON
//    ATTR1.NodeId = ROOT1.ParentNodeId OR 
//    ATTR1.NodeId = ROOT1.CreatedById OR 
//    ATTR1.NodeId = ROOT1.ModifiedById OR 
//    ATTR1.NodeId = ROOT1.LockedById 

//LEFT OUTER JOIN
//ReferenceProperties AS SLOT1			-- SLOT1 table branch (Level2)
//    INNER JOIN
//    SysSearchWithFlatsView AS REF1					-- Referenced by slot (Level2)
//    ON REF1.NodeId = SLOT1.ReferredNodeId 
//ON ROOT1.VersionId = SLOT1.VersionId

//LEFT OUTER JOIN 
//SysSearchWithFlatsView AS ROOT2 					-- output table (Level2)
//ON REF1.NodeId = ROOT2.NodeId OR ATTR1.NodeId = ROOT2.NodeId

//WHERE
//    ATTR0.NodeId = 1
//    OR
//    (SLOT0.PropertyTypeId = 3 AND REF0.Name = 'Gyeby')
//    OR
//    (SLOT0.PropertyTypeId = 7 AND SLOT1.PropertySlotId = 2 AND REF1.Name = 'Rita')

//         */

//        private enum LikeOperator { None, StartsWith, Contains, EndsWith }

//        private string CR;
//        private string _tabs;
//        private List<bool> _hasPropertyOnLevel;
//        private List<bool> _hasAttrReferenceOnLevel;
//        private List<bool> _hasSlotReferenceOnLevel;
//        //private List<int> _flatPages;
//        private List<MultiPage> _multiPageIndex;

//        private List<List<PropertyLiteral>> _referenceOnLevel;

//        private string _subfix = string.Empty;
//        private List<List<LevelSwitch>> _levelSwitchList = new List<List<LevelSwitch>>();
//        private List<List<LevelSwitch>> _levelAttrSwitchList = new List<List<LevelSwitch>>();

//        private SearchExpression _searchExpression;
//        private StringBuilder _querySb;
//        private StringBuilder _whereSb;
//        private StringBuilder _orderSb;
//        private StringBuilder _fromSb;
//        private StringBuilder _searchFooter;
//        private bool _hasFullTextSearch;
//        private string _orderNames = ",";
//        private List<string> _extraTableJoins;

//        static readonly string TAB = "\t";
//        static readonly string FULLTEXTJOIN1 = "LEFT OUTER JOIN {0} AS {1} ON {1}.VersionId = FTSEARCH.VersionId";
//        static readonly string FULLTEXTJOIN2 = "LEFT OUTER JOIN CONTAINSTABLE({0}, *, '{1}') AS {2} ON {2}.[key] = {3}.{4}";

//        static readonly string SELECTFT = "SELECT {1} ROW_NUMBER() OVER ({0}) AS RowOrder, FTSEARCH.NodeId, FTSEARCH.VersionId, FTSEARCH.MajorNumber, FTSEARCH.MinorNumber, FTSEARCH.Status, FTSEARCH.NodeTypeId, FTSEARCH.ContentListId, FTSEARCH.ContentListTypeId";
//        //static readonly string SELECT = "SELECT DISTINCT {0} NULL, ROOT0.NodeId, ROOT0.VersionId, ROOT0.MajorNumber, ROOT0.MinorNumber, ROOT0.Status, ROOT0.NodeTypeId, ROOT0.ContentListId, ROOT0.ContentListTypeId";
//        //static readonly string SELECTSTAR = "SELECT DISTINCT ROOT0.*";
//        static readonly string SELECTSTAR = "SELECT DISTINCT ROOT0.NodeId, ROOT0.VersionId, ROOT0.MajorNumber, ROOT0.MinorNumber, ROOT0.Status, ROOT0.NodeTypeId, ROOT0.ContentListId, ROOT0.ContentListTypeId{0}";

//        static readonly string CTESEARCH = "WITH Search_CTE (RowOrder , NodeId , VersionId , MajorNumber , MinorNumber , Status , NodeTypeId , ContentListId , ContentListTypeId ) AS (";

//        static readonly string TEMPSEARCHSELECTROW = ") SELECT * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY RowOrder) AS Row, NodeId, VersionId, MajorNumber, MinorNumber, Status, NodeTypeId, ContentListId, ContentListTypeId FROM ( SELECT DISTINCT ROW_NUMBER() OVER (partition by NodeId ORDER BY RowOrder) AS RowPart, * FROM Search_CTE WHERE Search_CTE.VersionId = dbo.udfGetVersionIdByNodeAndUserId(Search_CTE.NodeId, {0})) AS TEMPSEC WHERE TEMPSEC.RowPart = 1 ) AS TEMPWITHROW WHERE TEMPWITHROW.Row BETWEEN {1} and {2}";
//        static readonly string TEMPSEARCHSELECTORDER = ") SELECT RowOrder, NodeId, VersionId, MajorNumber, MinorNumber, Status, NodeTypeId, ContentListId, ContentListTypeId FROM ( SELECT ROW_NUMBER() OVER (PARTITION BY NodeId ORDER BY RowOrder) AS RowPart, RowOrder, NodeId, VersionId, MajorNumber, MinorNumber, Status, NodeTypeId, ContentListId, ContentListTypeId FROM Search_CTE WHERE Search_CTE.VersionId = dbo.udfGetVersionIdByNodeAndUserId(Search_CTE.NodeId, {0})) AS asd WHERE RowPart = 1 ORDER BY RowOrder";
//        static readonly string TEMPSEARCHSELECT = ") SELECT DISTINCT NodeId, VersionId, MajorNumber, MinorNumber, Status, NodeTypeId, ContentListId, ContentListTypeId FROM Search_CTE WHERE Search_CTE.VersionId = dbo.udfGetVersionIdByNodeAndUserId(Search_CTE.NodeId, {0})";

//        static readonly string SECURITYCHECK = "{0}.VersionId = dbo.udfGetVersionIdByNodeAndUserId({0}.NodeId, {1})";

//        static readonly string EXTRATABLEJOIN = "/*-EXTRATABLEJOIN {0}-*/";
//        static readonly string PAGEEQUALPAGEINDEX = "/*-PAGEEQUALPAGEINDEX {0}-*/";
//        static readonly string FLATPAGETABLEJOIN = "LEFT OUTER JOIN dbo.SysSearchWithFlatsView AS {1}_R{0} ON {1}_R{0}.Page = {0} AND ROOT0.NodeId = {1}_R{0}.NodeId AND ROOT0.Page <> {1}_R{0}.Page";

//        private Func<string, string> _resolver;

//        public SqlCompiler(Func<string, string> resolver)
//        {
//            _resolver = resolver;
//            CR = Environment.NewLine;
//            _tabs = TAB;
//        }

//        public SqlCompiler()
//        {
//            CR = Environment.NewLine;
//            _tabs = TAB;
//        }

//        private string ResolveLiteralValue(string literalValue)
//        {
//            if (_resolver == null)
//                return literalValue;
//            return _resolver(literalValue);
//        }

//        private List<NodeQueryParameter> _queryParameters;
//        private string CreateParameter(object value)
//        {
//            string paramName = "@p" + _queryParameters.Count;
//            _queryParameters.Add(new NodeQueryParameter { Name = paramName, Value = value });
//            return paramName;
//        }

//        public string Compile(NodeQuery query, out NodeQueryParameter[] parameters)
//        {
//            _queryParameters = new List<NodeQueryParameter>();

//            _hasPropertyOnLevel = new List<bool>();
//            _hasAttrReferenceOnLevel = new List<bool>();
//            _hasSlotReferenceOnLevel = new List<bool>();
//            //_flatPages = new List<int>();
//            _multiPageIndex = new List<MultiPage>();
//            _extraTableJoins = new List<string>();

//            _referenceOnLevel = new List<List<PropertyLiteral>>();

//            _hasPropertyOnLevel.Add(false);
//            _hasAttrReferenceOnLevel.Add(false);
//            _hasSlotReferenceOnLevel.Add(false);

//            _searchExpression = null;

//            _whereSb = new StringBuilder();
//            _querySb = new StringBuilder();
//            _fromSb = new StringBuilder();
//            _orderSb = new StringBuilder();
//            _searchFooter = new StringBuilder();

//            #region from Changeset #25077

//            //_querySb.Append("/*").Append(CR);
//            //_querySb.Append(query.ToXml()).Append(CR);
//            //_querySb.Append("*/").Append(CR);

//            #endregion


//            TreeWalker(query);

//            parameters = _queryParameters.ToArray();
//            return CreateQuery(query);
//        }

//        //SQL injection prevention
//        public static string SecureSqlStringValue(string value)
//        {
//            return value.Replace(@"'", @"''").Replace("/*", "**").Replace("--", "**");
//        }

//        private string CreateQuery(NodeQuery query)
//        {
//            //-- Search ----------------------------------------------------------------

//            _querySb.Append(CTESEARCH).Append(CR).Append(CR);

//            //-- Paging ----------------------------------------------------------------
//            bool paging = HasPaging(query);

//            //-- Order -----------------------------------------------------------------
//            bool hasOrder = CreateOrder(query, paging);

//            //-- Select ----------------------------------------------------------------
//            //if (true/*_hasFullTextSearch | hasOrder*/)
//            //{
//                if (string.IsNullOrEmpty(_orderSb.ToString()))
//                {
//                    _orderSb.Append("ORDER BY ").Append("FTSEARCH").Append(".NodeId");
//                }

//                _querySb.Append(String.Format(CultureInfo.InvariantCulture, SELECTFT, _orderSb, GetTop(query.Top))).Append(CR);
//                _querySb.Append("FROM").Append(CR);
//                _querySb.Append("(").Append(CR).Append(CR);

//                _querySb.Append(String.Format(CultureInfo.InvariantCulture, SELECTSTAR, _orderNames.TrimEnd(','))).Append(CR);
//            //}
//            //else
//            //{
//            //    _querySb.Append(string.Format(SELECT, GetTop(query.Top))).Append(CR);
//            //}

//            //-- From ------------------------------------------------------------------
//            CompileFromClause();
//            _querySb.Append(_fromSb).Append(CR);

//            //-- Where -----------------------------------------------------------------
//            if (_whereSb.ToString().Length > 6)
//            {
//                _querySb.Append("WHERE").Append(CR);
//                _querySb.Append(_whereSb).Append(CR);
//            }

//            if (true/*_hasFullTextSearch | hasOrder*/)
//            {
//                _querySb.Append(") AS FTSEARCH").Append(CR).Append(CR);
//                _querySb.Append(_searchFooter);
//            }

//            //-- Order ----------------------------------------------------------------
//            //_querySb.Append(_orderSb);

//            //-- Paging --------------------------------------------------------------
//            if (paging)
//            {
//                _querySb.Append(CR).Append(CR).AppendFormat(CultureInfo.InvariantCulture, TEMPSEARCHSELECTROW, AccessProvider.Current.GetCurrentUser().Id, query.StartIndex, query.StartIndex + query.PageSize - 1).Append(CR);
//            }
//            else
//            {
//                if (hasOrder)
//                {
//                    _querySb.Append(CR).Append(CR).AppendFormat(CultureInfo.InvariantCulture, TEMPSEARCHSELECTORDER, AccessProvider.Current.GetCurrentUser().Id).Append(CR);
//                }
//                else
//                {
//                    _querySb.Append(CR).Append(CR).AppendFormat(CultureInfo.InvariantCulture, TEMPSEARCHSELECT, AccessProvider.Current.GetCurrentUser().Id).Append(CR);
//                }
//            }

//            SetPageJoin();

//            return _querySb.ToString();
//        }

//        private void SetPageJoin()
//        {
//            foreach (MultiPage mp in _multiPageIndex)
//            {

//                string joins = string.Empty;
//                if (mp.IntList.Count == 1)
//                {
//                    if (mp.IntList[0] == 0)
//                        _querySb = _querySb.Replace(GetPageEqualString(mp.Name), string.Format(CultureInfo.InvariantCulture, "{1}.Page = {0} AND ", mp.IntList[0], mp.Name));
//                    else
//                        _querySb = _querySb.Replace(string.Concat(GetPageEqualString(mp.Name), mp.Name, "_R", mp.IntList[0]), string.Format(CultureInfo.InvariantCulture, "{1}.Page = {0} AND {1}", mp.IntList[0], mp.Name));
//                }
//                else
//                {
//                    foreach (int pageIndex in mp.IntList)
//                    {
//                        if (pageIndex != 0)
//                            joins = string.Concat(joins, string.Format(CultureInfo.InvariantCulture, FLATPAGETABLEJOIN, pageIndex, mp.Name), CR);
//                    }

//                    _querySb = _querySb.Replace(GetPageEqualString(mp.Name), string.Empty);
//                }
//                _querySb = _querySb.Replace(GetExtraTableJoinString(mp.Name), joins);
//            }
//            ClearPageJoinTemp();
//        }

//        private void ClearPageJoinTemp()
//        {
//            foreach (string extraTableJoinTemp in _extraTableJoins)
//            {
//                _querySb = _querySb.Replace(extraTableJoinTemp, string.Empty);
//            }
//        }

//        private static string GetTop(int top)
//        {
//            return top > 0 ? string.Concat("TOP ", top) : string.Empty;
//        }

//        private static bool HasPaging(NodeQuery query)
//        {
//            return query.StartIndex != 1 || query.PageSize != int.MaxValue;
//        }

//        //=========================================================================================== ORDER Clause

//        private bool CreateOrder(NodeQuery query, bool paging)
//        {
//            bool first = true;

//            if (paging)
//            {
//                if (query.Orders == null || query.Orders.Count < 1)
//                {
//                    _orderSb.Append("ORDER BY ").Append("FTSEARCH").Append(".NodeId");
//                    first = false;
//                }
//            }

//            foreach (SearchOrder order in query.Orders)
//            {
//                if (order.PropertyToOrder.IsSlot)
//                {
//                    _hasSlotReferenceOnLevel[0] = true;

//                    int pageIndex;
//                    int columnIndex;
//                    string columnPrefix;
//                    bool flatSlot;
//                    GetSlotMapping(order.PropertyToOrder.PropertySlot, out pageIndex, out columnIndex, out columnPrefix, out flatSlot);

//                    MultiPage mp = GetMultiPage("ROOT0");
//                    if (!mp.IntList.Contains(pageIndex))
//                        mp.IntList.Add(pageIndex);
//                }
//            }

//            foreach (SearchOrder order in query.Orders)
//            {
//                if (first)
//                {
//                    _orderSb.Append("ORDER BY ");
//                    first = false;
//                }
//                else
//                {
//                    _orderSb.Append(", ");
//                }

//                if (order.PropertyToOrder.IsSlot)
//                {
//                    int pageIndex;
//                    int columnIndex;
//                    string columnPrefix;
//                    bool flatSlot;
//                    GetSlotMapping(order.PropertyToOrder.PropertySlot, out pageIndex, out columnIndex, out columnPrefix, out flatSlot);

//                    MultiPage mp = GetMultiPage("ROOT0");

//                    if (mp.IntList.Count == 1)
//                    {
//                        //_orderSb.Append("FTSEARCH.[").Append(GetPropertySQLName(order.PropertyToOrder.PropertySlot)).Append("]");
//                        _orderNames = string.Concat(_orderNames, string.Format(CultureInfo.InvariantCulture, "ROOT0.{0}{1} AS ROOT0__{0}{1}", columnPrefix, columnIndex + 1), ",");
//                        _orderSb.AppendFormat(CultureInfo.InvariantCulture, "FTSEARCH.[ROOT0__{1}{2}]", pageIndex, columnPrefix, columnIndex + 1);
//                    }
//                    else
//                    {
//                        if (pageIndex == 0)
//                        {
//                            _orderNames = string.Concat(_orderNames, string.Format(CultureInfo.InvariantCulture, "ROOT0.{0}{1} AS ROOT0__{0}{1}", columnPrefix, columnIndex + 1), ",");
//                            _orderSb.AppendFormat(CultureInfo.InvariantCulture, "FTSEARCH.[ROOT0__{1}{2}]", pageIndex, columnPrefix, columnIndex + 1);
//                        }
//                        else
//                        {
//                            _orderNames = string.Concat(_orderNames, string.Format(CultureInfo.InvariantCulture, "ROOT0_R{0}.{1}{2} AS ROOT0_R{0}__{1}{2}", pageIndex, columnPrefix, columnIndex + 1), ",");
//                            _orderSb.AppendFormat(CultureInfo.InvariantCulture, "FTSEARCH.[ROOT0_R{0}__{1}{2}]", pageIndex, columnPrefix, columnIndex + 1);
//                        }
//                    }
//                }
//                else
//                {
//                    string na = GetNodeAttributeName(order.PropertyToOrder.NodeAttribute);
//                    _orderSb.Append("FTSEARCH.[").Append(na).Append("]");
//                    if (!SELECTSTAR.Contains(string.Concat("ROOT0.", na)))
//                        _orderNames = string.Concat(_orderNames, "ROOT0.[", na, "],");
//                }
//                _orderSb.Append(order.Direction == OrderDirection.Desc ? " DESC" : string.Empty);
//            }

//            return !first;
//        }

//        //private string GetPropertySQLName(PropertyType propertyType)
//        //{
//        //    switch (propertyType.DataType)
//        //    {
//        //        case DataType.String: return string.Concat("nvarchar_", GetSlotNumber(propertyType.Mapping + 1, SqlProvider.StringPageSize));
//        //        case DataType.Int: return string.Concat("int_", GetSlotNumber(propertyType.Mapping + 1, SqlProvider.IntPageSize));
//        //        case DataType.DateTime: return string.Concat("datetime_", GetSlotNumber(propertyType.Mapping + 1, SqlProvider.DateTimePageSize));
//        //        case DataType.Currency: return string.Concat("money_", GetSlotNumber(propertyType.Mapping + 1, SqlProvider.CurrencyPageSize));
//        //        default: throw new ApplicationException(string.Concat("Invalid search DataType PropertyType: ", propertyType.Name, ", DataType: ", propertyType.DataType));
//        //    }
//        //}

//        //private int GetSlotNumber(int num, int div)
//        //{
//        //    int result;
//        //    Math.DivRem(num, div, out result);
//        //    return result;
//        //}

//        //private string GetRootTable()
//        //{
//        //    return _hasFullTextSearch ? "FTSEARCH" : "ROOT0";
//        //}

//        private static string GetNodeAttributeName(NodeAttribute attr)
//        {
//            switch (attr)
//            {
//                case NodeAttribute.Id:
//                    return "NodeId";
//                case NodeAttribute.IsDeleted:
//                    return "IsDeleted";
//                case NodeAttribute.IsInherited:
//                    return "IsInherited";
//                case NodeAttribute.ParentId:
//                case NodeAttribute.Parent:
//                    return "ParentNodeId";
//                case NodeAttribute.Name:
//                    return "Name";
//                case NodeAttribute.Path:
//                    return "Path";
//                case NodeAttribute.Index:
//                    return "Index";
//                case NodeAttribute.Locked:
//                    return "Locked";
//                case NodeAttribute.LockedById:
//                case NodeAttribute.LockedBy:
//                    return "LockedById";
//                case NodeAttribute.ETag:
//                    return "ETag";
//                case NodeAttribute.LockType:
//                    return "LockType";
//                case NodeAttribute.LockTimeout:
//                    return "LockTimeout";
//                case NodeAttribute.LockDate:
//                    return "LockDate";
//                case NodeAttribute.LockToken:
//                    return "LockToken";
//                case NodeAttribute.LastLockUpdate:
//                    return "LastLockUpdate";
//                case NodeAttribute.MajorVersion:
//                    return "MajorNumber";
//                case NodeAttribute.MinorVersion:
//                    return "MinorNumber";
//                case NodeAttribute.CreationDate:
//                    return "CreationDate";
//                case NodeAttribute.CreatedById:
//                case NodeAttribute.CreatedBy:
//                    return "CreatedById";
//                case NodeAttribute.ModificationDate:
//                    return "ModificationDate";
//                case NodeAttribute.ModifiedById:
//                case NodeAttribute.ModifiedBy:
//                    return "ModifiedById";
//                case NodeAttribute.FullTextRank:
//                    return "FullTextRank";
//                default:
//                    throw new NotImplementedException(String.Concat(
//                        "SenseNet.ContentRepository.Storage.Data.SqlClient.GetNodeAttributeName(NodeAttribute attr = ", attr, ")"));
//            }
//        }

//        //=========================================================================================== FROM Clause

//        private void CompileFromClause()
//        {
//            int levels = _hasPropertyOnLevel.Count;
//            while (_hasAttrReferenceOnLevel.Count < levels)
//                _hasAttrReferenceOnLevel.Add(false);
//            while (_hasSlotReferenceOnLevel.Count < levels)
//                _hasSlotReferenceOnLevel.Add(false);

//            _querySb.Append("FROM").Append(CR);

//            ///*<MultiPage>*/
//            //if (_flatPages.Count > 1)
//            //{
//            //    _querySb.Append("/* ");
//            //    for (var i = 1; i < _flatPages.Count; i++)
//            //        _querySb.Append(", FlatProperties AS FlatPage").Append(_flatPages[i]);
//            //    _querySb.Append(" */").Append(CR);
//            //}
//            ///*</MultiPage>*/

//            for (int level = 0; level < levels; level++)
//            {
//                if (levels > 1)
//                    _querySb.Append(CR).Append("---- Level #").Append(level).Append(CR).Append(CR);
//                if (level == 0)
//                {
//                    CompileTableByLevel(level, "ROOT", _hasSlotReferenceOnLevel[0]);
//                    string extraJoinTemp = GetExtraTableJoinString(string.Concat("ROOT", level));
//                    _querySb.Append(extraJoinTemp);
//                    _extraTableJoins.Add(extraJoinTemp);
//                }
//                else
//                    CompileReferenceJoins(level);
//            }
//        }

//        private void CompileReferenceJoins(int level)
//        {
//            if (_hasAttrReferenceOnLevel[level])
//            {
//                CompileAttrReferenceJoin(level - 1);
//            }
//            if (_hasSlotReferenceOnLevel[level])
//            {
//                CompileSlotReferenceJoin(level - 1);
//            }
//        }

//        private void CompileAttrReferenceJoin(int level)
//        {
//            int i = 0;
//            foreach (LevelSwitch ls in _levelAttrSwitchList[level])
//            {
//                if (!ls.PropLit.IsSlot)
//                {
//                    switch (ls.PropLit.NodeAttribute)
//                    {
//                        case NodeAttribute.Parent:
//                            {
//                                AttrJoin(level, string.Concat("ATTR_PARENT_", ls.Subfix, "_"), ".ParentNodeId", GetSpecificAttrReferenceOnLevel(level - 1, ls.Subfix), ls.IsSlot);
//                                break;
//                            }
//                        case NodeAttribute.CreatedBy:
//                            {
//                                AttrJoin(level, string.Concat("ATTR_CREATEDBY_", ls.Subfix, "_"), ".CreatedById", GetSpecificAttrReferenceOnLevel(level - 1, ls.Subfix), ls.IsSlot);
//                                break;
//                            }
//                        case NodeAttribute.ModifiedBy:
//                            {
//                                AttrJoin(level, string.Concat("ATTR_MODIFIEDBY_", ls.Subfix, "_"), ".ModifiedById", GetSpecificAttrReferenceOnLevel(level - 1, ls.Subfix), ls.IsSlot);
//                                break;
//                            }
//                        case NodeAttribute.LockedBy:
//                            {
//                                AttrJoin(level, string.Concat("ATTR_LOCKEDBY_", ls.Subfix, "_"), ".LockedById", GetSpecificAttrReferenceOnLevel(level - 1, ls.Subfix), ls.IsSlot);
//                                break;
//                            }
//                    }
//                }
//                i++;
//            }
//        }

//        private string GetSpecificAttrReferenceOnLevel(int level, string subfix)
//        {
//            if (level >= 0 && _levelAttrSwitchList != null && _levelAttrSwitchList.Count >= level)
//            {
//                foreach (LevelSwitch ls in _levelAttrSwitchList[level])
//                {
//                    if (!ls.PropLit.IsSlot && ls.Subfix == subfix)
//                    {
//                        switch (ls.PropLit.NodeAttribute)
//                        {
//                            case NodeAttribute.Parent:
//                                return string.Concat("ATTR_PARENT_", ls.Subfix, "_", level);
//                            case NodeAttribute.CreatedBy:
//                                return string.Concat("ATTR_CREATEDBY_", ls.Subfix, "_", level);
//                            case NodeAttribute.ModifiedBy:
//                                return string.Concat("ATTR_MODIFIEDBY_", ls.Subfix, "_", level);
//                            case NodeAttribute.LockedBy:
//                                return string.Concat("ATTR_LOCKEDBY_", ls.Subfix, "_", level);
//                        }
//                    }
//                }
//            }
//            return string.Empty;
//        }

//        private void AttrJoin(int level, string attr, string propName, string lastRoot, bool isFlat)
//        {
//            if (level == 0)
//            {
//                lastRoot = "ROOT0";
//            }
//            _querySb.Append("LEFT OUTER JOIN").Append(CR);
//            CompileTableByLevel(level, attr, isFlat);
//            _querySb.Append("ON").Append(CR);
//            _querySb.Append("\t").Append(attr).Append(level).Append(".NodeId = ").Append(lastRoot).Append(propName).Append(" AND ").AppendFormat(CultureInfo.InvariantCulture, SECURITYCHECK, string.Concat(attr, level), AccessProvider.Current.GetCurrentUser().Id).Append(CR).Append(CR);

//            string extraJoinTemp = GetExtraTableJoinString(string.Concat(attr, level));
//            _querySb.Append(extraJoinTemp);
//            _extraTableJoins.Add(extraJoinTemp);
//        }

//        private void CompileSlotReferenceJoin(int level)
//        {
//            foreach (LevelSwitch levelSwitch in _levelSwitchList[level])
//            {
//                string reference = String.Concat("REF", "_", levelSwitch.Subfix, "_");
//                string @ref = String.Concat(reference, level);
//                string slot = String.Concat("SLOT", "_", levelSwitch.Subfix, "_", level);
//                string comment = String.Concat("\t\t\t\t-- Reference table branch (Level", level + 1, ")");

//                _querySb.Append("LEFT OUTER JOIN").Append(CR);
//                _querySb.Append("ReferenceProperties AS ").Append(slot).Append(comment).Append(CR);
//                _querySb.Append("\tINNER JOIN").Append(CR);
//                _querySb.Append("\t");
//                CompileTableByLevel(level, reference, levelSwitch.IsSlot);
//                _querySb.Append("\tON ").Append(@ref).Append(".NodeId = ").Append(slot).Append(".ReferredNodeId AND ").AppendFormat(CultureInfo.InvariantCulture, SECURITYCHECK, @ref, AccessProvider.Current.GetCurrentUser().Id).Append(CR);
//                _querySb.Append("ON ").Append(level == 0 ? "ROOT0" : String.Concat(reference, level - 1)).Append(".VersionId = ").Append(slot).Append(".VersionId").Append(CR).Append(CR);

//                string extraJoinTemp = GetExtraTableJoinString(string.Concat(reference, level));
//                _querySb.Append(extraJoinTemp);
//                _extraTableJoins.Add(extraJoinTemp);
//            }
//        }

//        //private void CompileTableByLevel(int level, string alias)
//        //{
//        //    CompileTableByLevel(level, alias, false);
//        //}

//        private void CompileTableByLevel(int level, string alias, bool isSlot)
//        {
//            string comment = "";
//            switch (alias)
//            {
//                case "ROOT":
//                    if (level == 0)
//                        comment = "\t\t\t\t\t\t\t-- Root table";
//                    else
//                        comment = String.Concat("\t\t\t\t\t-- Connector table (Level", level, "-", level + 1, ")");
//                    break;
//                case "ATTR":
//                    comment = String.Concat("\t\t\t\t\t-- Attribute table branch (Level", level + 1, ")");
//                    break;
//                case "SLOT":
//                    comment = String.Concat("\t\t\t\t\t-- Reference table branch (Level", level + 1, ")");
//                    break;
//                case "REF":
//                    comment = String.Concat("\t\t\t\t-- Referenced by reference table (Level", level + 1, ")");
//                    break;
//            }
//            if (isSlot)
//                _querySb.Append("dbo.SysSearchWithFlatsView");
//            else
//                _querySb.Append("dbo.SysSearchView");
//            _querySb.Append(" AS ").Append(alias).Append(level).Append(comment).Append(CR);

//            //_querySb.Append(GetExtraTableJoinString(string.Concat(alias, level)));
//        }

//        //=========================================================================================== Tree walker

//        private void TreeWalker(Expression exp)
//        {
//            CompileExpressionNode(exp, new List<PropertyLiteral>(), 0);
//        }
//        private void CompileExpressionNode(Expression expression, List<PropertyLiteral> refChain, int indent)
//        {
//            ExpressionList expList;
//            NotExpression notExp;
//            ReferenceExpression refExp;
//            SearchExpression textExp;
//            TypeExpression typeExp;
//            IBinaryExpressionWrapper binExp;

//            if ((expList = expression as ExpressionList) != null)
//                CompileExpressionListNode(expList, refChain, indent);
//            else if ((notExp = expression as NotExpression) != null)
//                CompileNotExpressionNode(notExp, refChain, indent);
//            else if ((refExp = expression as ReferenceExpression) != null)
//                CompileReferenceExpressionNode(refExp, refChain, indent);
//            else if ((textExp = expression as SearchExpression) != null)
//                CompileSearchExpressionNode(textExp);
//            else if ((typeExp = expression as TypeExpression) != null)
//                CompileTypeExpressionNode(typeExp, refChain, indent);
//            else if ((binExp = expression as IBinaryExpressionWrapper) != null)
//                CompileBinaryExpressionNode(binExp, refChain, indent);
//        }

//        private void CompileExpressionListNode(ExpressionList expression, List<PropertyLiteral> refChain, int indent)
//        {
//            int expCount = expression.Expressions.Count;
//            if (expCount == 0)
//                return;
//            _whereSb.Append(Indent(indent)).Append("(").Append(CR);

//            int subfixLevel = 1;

//            bool first = true;
//            foreach (Expression expr in expression.Expressions)
//            {
//                if (expr is SearchExpression)
//                {
//                    CompileExpressionNode(expr, null, 0);
//                }
//                else
//                {
//                    //-- List operator (AND|OR)
//                    if (first)
//                        first = false;
//                    else
//                        _whereSb.Append(Indent(indent + 1)).Append(expression.OperatorType == ChainOperator.And ? "AND" : "OR").Append(CR);

//                    _subfix = string.Concat(_subfix, subfixLevel.ToString(CultureInfo.InvariantCulture));

//                    //-- render parenthesis if needed
//                    if (expCount > 1)
//                    {
//                        _whereSb.Append(Indent(indent + 1)).Append("(").Append(CR);
//                        CompileExpressionNode(expr, refChain, indent + 2);
//                        _whereSb.Append(Indent(indent + 1)).Append(")").Append(CR);
//                    }
//                    else
//                    {
//                        CompileExpressionNode(expr, refChain, indent + 1);
//                    }

//                    subfixLevel++;
//                    _subfix = _subfix.Remove(_subfix.Length - 1);
//                }
//            }

//            _whereSb.Append(Indent(indent)).Append(")").Append(CR);
//        }
//        private void CompileNotExpressionNode(NotExpression expression, List<PropertyLiteral> refChain, int indent)
//        {
//            _whereSb.Append(Indent(indent)).Append("NOT").Append(CR);
//            _whereSb.Append(Indent(indent)).Append("(").Append(CR);
//            CompileExpressionNode(expression.Expression, refChain, indent + 1);
//            _whereSb.Append(Indent(indent)).Append(")").Append(CR);
//        }
//        private void CompileSearchExpressionNode(SearchExpression expression)
//        {
//            if (!(expression.Parent is NodeQuery))
//                throw new NotSupportedException();
//            if (_searchExpression != null)
//                throw new NotSupportedException();
//            _searchExpression = expression;
//            if (_hasFullTextSearch)
//                throw new NotSupportedException("More than one fulltext expression!");

//            _hasFullTextSearch = true;

//            //-- FROM --
//            FullTextJoin("-- Binary --", expression.FullTextExpression, "dbo.BinaryProperties", "TPB", "B", "BinaryPropertyId");
//            FullTextJoin("-- Flat --", expression.FullTextExpression, "dbo.FlatProperties", "TPF", "F", "Id");
//            FullTextJoin("-- Text --", expression.FullTextExpression, "dbo.TextPropertiesNText", "TPT", "T", "TextPropertyNTextId");
//            FullTextJoin("-- NVarchar --", expression.FullTextExpression, "dbo.TextPropertiesNVarchar", "TPV", "V", "TextPropertyNVarcharId");

//            _searchFooter.Append("-- Node --");
//            _searchFooter.Append(CR);
//            _searchFooter.Append(string.Format(CultureInfo.InvariantCulture, FULLTEXTJOIN2, "dbo.Nodes", expression.FullTextExpression, "N", "FTSEARCH", "NodeId"));

//            //-- WHERE --
//            _searchFooter.Append(CR).Append(CR).Append("WHERE").Append(CR).Append(TAB).Append("(").Append(CR);

//            FullTextWhere("B");
//            FullTextWhere("F");
//            FullTextWhere("T");
//            FullTextWhere("V");
//            FullTextWhere("N", true);

//            _searchFooter.Append(TAB).Append(")").Append(CR);
//        }

//        private void FullTextJoin(string title, string keyword, string tblFullName, string tblName, string tblShortName, string key)
//        {
//            _searchFooter.Append(title);
//            _searchFooter.Append(CR);
//            _searchFooter.Append(string.Format(CultureInfo.InvariantCulture, FULLTEXTJOIN1, tblFullName, tblName));
//            _searchFooter.Append(CR);
//            _searchFooter.Append(string.Format(CultureInfo.InvariantCulture, FULLTEXTJOIN2, tblFullName, keyword, tblShortName, tblName, key));
//            _searchFooter.Append(CR).Append(CR);
//        }

//        private void FullTextWhere(string tblShortName)
//        {
//            FullTextWhere(tblShortName, false);
//        }
//        private void FullTextWhere(string tblShortName, bool last)
//        {
//            _searchFooter.Append(TAB).Append(TAB).Append(tblShortName).Append(".[key] IS NOT NULL").Append(CR);
//            if (!last)
//            {
//                _searchFooter.Append(TAB).Append(TAB).Append("OR").Append(CR);
//            }
//        }

//        private void CompileTypeExpressionNode(TypeExpression expression, List<PropertyLiteral> refChain, int indent)
//        {
//            List<string> idList = new List<string>();
//            NodeType type = expression.NodeType;
//            _whereSb.Append(Indent(indent));

//            //if (expression.ExactMatch)
//            //    idList.Add(type.Id.ToString(CultureInfo.InvariantCulture));
//            //else
//            //    foreach (NodeType subType in type.GetAllTypes())
//            //        idList.Add(subType.Id.ToString(CultureInfo.InvariantCulture));

//            if (expression.ExactMatch)
//                idList.Add(CreateParameter(type.Id));
//            else
//                foreach (NodeType subType in type.GetAllTypes())
//                    idList.Add(CreateParameter(subType.Id));

//            if (idList.Count == 1)
//            {
//                CompileLiteralAlias(refChain, false);
//                _whereSb.Append("NodeTypeId = ").Append(idList[0]);
//            }
//            else
//            {
//                CompileLiteralAlias(refChain, false);
//                _whereSb.Append("NodeTypeId IN (");
//                _whereSb.Append(String.Join(", ", idList.ToArray()));
//                _whereSb.Append(")");
//            }
//            _whereSb.Append(CR);
//        }
//        private void CompileReferenceExpressionNode(ReferenceExpression expression, List<PropertyLiteral> refChain, int indent)
//        {
//            List<PropertyLiteral> newRefChain = new List<PropertyLiteral>(refChain);
//            newRefChain.Add(expression.ReferrerProperty);

//            _whereSb.Append(Indent(indent));
//            if (expression.Expression != null)
//            {
//                _whereSb.Append("-- ReferenceExpression: Expression: ");
//                CompilePropertyLiteral(expression.ReferrerProperty, newRefChain);
//                _whereSb.Append(CR);
//                CompileExpressionNode(expression.Expression, newRefChain, indent);
//            }
//            else if (expression.ReferencedNode != null)
//            {
//                //-- "SLOT0.PropertyTypeId = 26 AND REF0.NodeId = 2"
//                if (expression.ReferrerProperty.IsSlot)
//                    _whereSb.Append("-- ReferenceExpression: ReferencedNode, PropertyTypeId = ").Append(expression.ReferrerProperty.PropertySlot.Id).Append(", NodeId = ").Append(expression.ReferencedNode.Id);
//                else
//                    _whereSb.Append("-- ReferenceExpression: ReferencedNode, NodeAttribute = ").Append(expression.ReferrerProperty.NodeAttribute).Append(", NodeId = ").Append(expression.ReferencedNode.Id);
//                _whereSb.Append(CR);//.Append(Indent(indent));
//                CompileReferenceChain(newRefChain, indent, false);
//                _whereSb.Append(Indent(indent));
//                CompileLiteralAlias(newRefChain, expression.ReferrerProperty);
//                _whereSb.Append("NodeId = ");
//                _whereSb.Append(expression.ReferencedNode.Id);
//                _whereSb.Append(CR);
//            }
//            else
//            {
//                //-- expression.ExistenceOnly = true
//                //-- "SLOT0.PropertyTypeId = 26"
//                _whereSb.Append("-- ReferenceExpression: ExistenceOnly, PropertyTypeId = ").Append(expression.ReferrerProperty.PropertySlot.Id).Append(CR);
//                CompileReferenceChain(newRefChain, indent, true);
//            }
//        }
//        private void CompileBinaryExpressionNode(IBinaryExpressionWrapper expression, List<PropertyLiteral> refChain, int indent)
//        {
//            // left op right
//            PropertyLiteral left = expression.BinExp.LeftValue;
//            Operator op = expression.BinExp.Operator;
//            Literal right = expression.BinExp.RightValue;

//            CompileReferenceChain(refChain, indent, false, left.IsSlot);

//            _whereSb.Append(Indent(indent));
//            CompilePropertyLiteral(left, refChain);
//            if (op == Operator.StartsWith || op == Operator.Contains || op == Operator.EndsWith)
//            {
//                CompileLikeOperator(op, right, refChain);
//            }
//            else
//            {
//                if (right.IsValue && right.Value == null)
//                {
//                    switch (op)
//                    {
//                        case Operator.Equal:
//                            _whereSb.Append(" IS NULL ");
//                            break;
//                        case Operator.NotEqual:
//                            _whereSb.Append(" IS NOT NULL ");
//                            break;
//                        default:
//                            throw new NotSupportedException(String.Concat("The ", op, " operator is not supported with a NULL value. Only the Equal and NotEqual operators supported, which are translated to IS NULL and IS NOT NULL expressions."));
//                    }
//                }
//                else
//                {
//                    CompileOperator(op);
//                    CompileLiteral(right, refChain, LikeOperator.None);
//                }
//            }
//            _whereSb.Append(CR);
//        }

//        private void CompileReferenceChain(List<PropertyLiteral> refChain, int indent, bool chainOnly)
//        {
//            CompileReferenceChain(refChain, indent, chainOnly, false);
//        }

//        private void CompileReferenceChain(List<PropertyLiteral> refChain, int indent, bool chainOnly, bool isFlat)
//        {
//            if (refChain.Count == 0)
//            {
//                if (isFlat)
//                {
//                    LevelHasSlotReference(0);
//                }
//                return;
//            }

//            string lastAttr = string.Empty;

//            for (int i = 0; i < refChain.Count; i++)
//            {
//                PropertyLiteral propLit = refChain[i];
//                _whereSb.Append(Indent(indent));
//                if (propLit.IsSlot)
//                {
//                    LevelHasSlotReference(i + 1);

//                    AddSlotRefOnLevel(i, _subfix, isFlat && i == refChain.Count - 1);

//                    _whereSb.Append("SLOT").Append("_").Append(_subfix).Append("_").Append(i);
//                    _whereSb.Append(".PropertyTypeId = ");
//                    _whereSb.Append(propLit.PropertySlot.Id);
//                }
//                else
//                {
//                    // ATTRx.NodeId = ROOTx.???
//                    LevelHasAttrReference(i + 1);

//                    AddAttrRefOnLevel(i, _subfix, refChain, isFlat);

//                    if (_referenceOnLevel.Count <= i)
//                    {
//                        _referenceOnLevel.Add(new List<PropertyLiteral>());
//                    }
//                    _referenceOnLevel[i].Add(refChain[i]);

//                    string attr = GetAttr(refChain[i].NodeAttribute);

//                    string root = string.Empty;
//                    if (i > 0)
//                    {
//                        root = string.Concat(lastAttr, _subfix, "_", i - 1);
//                    }
//                    else
//                    {
//                        root = "ROOT0";
//                    }

//                    _whereSb.Append(attr).Append(_subfix).Append("_").Append(i).Append(".NodeId = ").Append(root).Append(".");
//                    CompileNodeAttributeName(propLit.NodeAttribute);

//                    lastAttr = attr;
//                }
//                if ((i < refChain.Count - 1) || !chainOnly)
//                    _whereSb.Append(CR).Append(Indent(indent)).Append("AND");
//                _whereSb.Append(CR);
//            }
//        }

//        private void AddAttrRefOnLevel(int level, string subfix, List<PropertyLiteral> refChain, bool isFlat)
//        {
//            if (_levelAttrSwitchList.Count <= level)
//            {
//                _levelAttrSwitchList.Add(new List<LevelSwitch>());
//            }
//            LevelSwitch ls = new LevelSwitch(subfix);
//            ls.IsSlot = isFlat;
//            ls.PropLit = refChain[level];
//            _levelAttrSwitchList[level].Add(ls);
//        }

//        private void AddSlotRefOnLevel(int level, string subfix, bool isSlot)
//        {
//            if (_levelSwitchList.Count <= level)
//            {
//                _levelSwitchList.Add(new List<LevelSwitch>());
//            }
//            _levelSwitchList[level].Add(new LevelSwitch(subfix, isSlot));
//        }

//        private static string GetAttr(NodeAttribute nodeAttr)
//        {
//            switch (nodeAttr)
//            {
//                case NodeAttribute.Parent: return "ATTR_PARENT_";
//                case NodeAttribute.CreatedBy: return "ATTR_CREATEDBY_";
//                case NodeAttribute.ModifiedBy: return "ATTR_MODIFIEDBY_";
//                case NodeAttribute.LockedBy: return "ATTR_LOCKEDBY_";
//                default: return "ATTR";
//            }
//        }

//        private void CompilePropertyLiteral(PropertyLiteral propLit, List<PropertyLiteral> refChain)
//        {
//            if (propLit.IsSlot)
//                CompilePropertySlot(propLit.PropertySlot, refChain);
//            else
//                CompileNodeAttribute(propLit.NodeAttribute, refChain);
//        }
//        private void CompileOperator(Operator op)
//        {
//            switch (op)
//            {
//                case Operator.Equal:
//                    _whereSb.Append(" = ");
//                    break;
//                case Operator.NotEqual:
//                    _whereSb.Append(" != ");
//                    break;
//                case Operator.LessThan:
//                    _whereSb.Append(" < ");
//                    break;
//                case Operator.GreaterThan:
//                    _whereSb.Append(" > ");
//                    break;
//                case Operator.LessThanOrEqual:
//                    _whereSb.Append(" <= ");
//                    break;
//                case Operator.GreaterThanOrEqual:
//                    _whereSb.Append(" >= ");
//                    break;
//                default:
//                    break;
//            }
//        }
//        private void CompileLikeOperator(Operator op, Literal right, List<PropertyLiteral> refChain)
//        {
//            switch (op)
//            {
//                case Operator.StartsWith:
//                    //_whereSb.Append(" LIKE N'");
//                    _whereSb.Append(" LIKE ");
//                    CompileLiteral(right, refChain, LikeOperator.StartsWith);
//                    //_whereSb.Append("%'");
//                    break;
//                case Operator.EndsWith:
//                    //_whereSb.Append(" LIKE N'%");
//                    _whereSb.Append(" LIKE ");
//                    CompileLiteral(right, refChain, LikeOperator.EndsWith);
//                    //_whereSb.Append("'");
//                    break;
//                case Operator.Contains:
//                    //_whereSb.Append(" LIKE N'%");
//                    _whereSb.Append(" LIKE ");
//                    CompileLiteral(right, refChain, LikeOperator.Contains);
//                    //_whereSb.Append("%'");
//                    break;
//                default:
//                    break;
//            }
//        }
//        private void CompileLiteral(Literal lit, List<PropertyLiteral> refChain, LikeOperator likeOperator)
//        {
//            if (lit.IsValue)
//                CompileLiteralValue(lit.Value, likeOperator);
//            else
//                CompilePropertyLiteral(lit, refChain);
//        }
//        private void CompilePropertySlot(PropertyType slot, List<PropertyLiteral> refChain)
//        {
//            if (slot.DataType != DataType.Reference)
//            {
//                LevelHasPropertySlot(refChain.Count);
//            }
//            CompileSlotMapping(slot, GetSlotAlias(refChain, _subfix));
//        }
//        private void CompileNodeAttribute(NodeAttribute attr, List<PropertyLiteral> refChain)
//        {
//            CompileAttrAlias(refChain);
//            CompileNodeAttributeName(attr);
//        }
//        private void CompileNodeAttributeName(NodeAttribute attr)
//        {
//            switch (attr)
//            {
//                case NodeAttribute.Id:
//                    _whereSb.Append("NodeId");
//                    break;
//                case NodeAttribute.IsDeleted:
//                    _whereSb.Append("IsDeleted");
//                    break;
//                case NodeAttribute.IsInherited:
//                    _whereSb.Append("IsInherited");
//                    break;
//                case NodeAttribute.ParentId:
//                case NodeAttribute.Parent:
//                    _whereSb.Append("ParentNodeId");
//                    break;
//                case NodeAttribute.Name:
//                    _whereSb.Append("Name");
//                    break;
//                case NodeAttribute.Path:
//                    _whereSb.Append("Path");
//                    break;
//                case NodeAttribute.Index:
//                    _whereSb.Append("[Index]");
//                    break;
//                case NodeAttribute.Locked:
//                    _whereSb.Append("Locked");
//                    break;
//                case NodeAttribute.LockedById:
//                case NodeAttribute.LockedBy:
//                    _whereSb.Append("LockedById");
//                    break;
//                case NodeAttribute.ETag:
//                    _whereSb.Append("ETag");
//                    break;
//                case NodeAttribute.LockType:
//                    _whereSb.Append("LockType");
//                    break;
//                case NodeAttribute.LockTimeout:
//                    _whereSb.Append("LockTimeout");
//                    break;
//                case NodeAttribute.LockDate:
//                    _whereSb.Append("LockDate");
//                    break;
//                case NodeAttribute.LockToken:
//                    _whereSb.Append("LockToken");
//                    break;
//                case NodeAttribute.LastLockUpdate:
//                    _whereSb.Append("LastLockUpdate");
//                    break;
//                case NodeAttribute.MajorVersion:
//                    _whereSb.Append("MajorNumber");
//                    break;
//                case NodeAttribute.MinorVersion:
//                    _whereSb.Append("MinorNumber");
//                    break;
//                case NodeAttribute.CreationDate:
//                    _whereSb.Append("CreationDate");
//                    break;
//                case NodeAttribute.CreatedById:
//                case NodeAttribute.CreatedBy:
//                    _whereSb.Append("CreatedById");
//                    break;
//                case NodeAttribute.ModificationDate:
//                    _whereSb.Append("ModificationDate");
//                    break;
//                case NodeAttribute.ModifiedById:
//                case NodeAttribute.ModifiedBy:
//                    _whereSb.Append("ModifiedById");
//                    break;
//                case NodeAttribute.FullTextRank:
//                    _whereSb.Append("FullTextRank");
//                    break;
//                default:
//                    throw new NotImplementedException(String.Concat(
//                        "SenseNet.ContentRepository.Storage.Data.SqlClient.CompileNodeAttributeName(NodeAttribute attr = ", attr, ")"));
//            }
//        }
//        private void CompileLiteralValue(object value, LikeOperator likeOperator)
//        {
//            var stringValue = value as string;
//            if (stringValue != null)
//                _whereSb.Append(CreateParameter(PrepareLikeValue(ResolveLiteralValue(stringValue), likeOperator)));
//            else
//                _whereSb.Append(CreateParameter(value));

//            //if (value is DateTime)
//            //{
//            //    _whereSb.Append("'").Append(((DateTime)value).ToString(CultureInfo.InvariantCulture)).Append("'");
//            //}
//            //else if(value is string)
//            //{
//            //    if(likeOperator)
//            //        _whereSb.Append(ResolveLiteralValue(SecureSqlStringValue((string)value)));
//            //    else
//            //        _whereSb.Append("N'").Append(ResolveLiteralValue(SecureSqlStringValue((string)value))).Append("'");
//            //}
//            //else // decimal | int | stb.
//            //{
//            //    _whereSb.Append(value);
//            //}
//        }
//        private void CompileLiteralAlias(List<PropertyLiteral> refChain, PropertyLiteral literal)
//        {
//            CompileLiteralAlias(refChain, literal.IsSlot);
//        }
//        private void CompileLiteralAlias(List<PropertyLiteral> refChain, bool isSlot)
//        {
//            int levelIndex = refChain.Count - 1;
//            if (levelIndex == -1)
//            {
//                _whereSb.Append("ROOT0.");
//                return;
//            }

//            //_whereSb.Append(isSlot ? "REF" : "ATTR").Append("_").Append(_subfix).Append("_").Append(levelIndex).Append(".");
//            if(isSlot)
//                _whereSb.Append("REF").Append("_").Append(_subfix).Append("_").Append(levelIndex).Append(".");
//            else
//                _whereSb.Append(GetAttr(refChain[levelIndex].NodeAttribute)).Append(_subfix).Append("_").Append(levelIndex).Append(".");
//        }
//        private void CompileAttrAlias(List<PropertyLiteral> refChain)
//        {
//            int levelIndex = refChain.Count - 1;
//            if (levelIndex == -1)
//            {
//                _whereSb.Append("ROOT0.");
//                return;
//            }
//            if (refChain[levelIndex].IsSlot)
//            {
//                _whereSb.Append("REF").Append("_").Append(_subfix).Append("_").Append(levelIndex).Append(".");
//            }
//            else
//            {
//                string attr = GetAttr(refChain[levelIndex].NodeAttribute);
//                _whereSb.Append(attr).Append(_subfix).Append("_").Append(levelIndex).Append(".");
//            }
//        }
//        private void CompileSlotMapping(PropertyType slot, string alias)
//        {
//            int pageIndex;
//            int columnIndex;
//            string columnPrefix;
//            bool flatSlot;
//            GetSlotMapping(slot, out pageIndex, out columnIndex, out columnPrefix, out flatSlot);
//            if (flatSlot)
//            {
//                ///*<MultiPage>*/
//                //if (!_flatPages.Contains(pageIndex))
//                //    _flatPages.Add(pageIndex);
//                //_whereSb.Append("/* ").Append("FlatPage").Append(pageIndex).Append("_ */");
//                ///*</MultiPage>*/

//                //-- e.g.: REF2.Page = 1 AND REF2.nvarchar_1
//                //_whereSb.Append(alias).Append("Page = ").Append(pageIndex).Append(" AND ");
//                _whereSb.Append(GetPageEqualString(alias.TrimEnd('.')));
//                if (pageIndex == 0)
//                    _whereSb.Append(alias).Append(columnPrefix).Append(columnIndex + 1);
//                else
//                    _whereSb.Append(alias.TrimEnd('.')).Append("_R").Append(pageIndex).Append(".").Append(columnPrefix).Append(columnIndex + 1);

//                MultiPage mp = GetMultiPage(alias.TrimEnd('.'));
//                if (!mp.IntList.Contains(pageIndex))
//                    mp.IntList.Add(pageIndex);
//            }
//        }

//        private static void GetSlotMapping(PropertyType slot, out int pageIndex, out int columnIndex, out string columnPrefix, out bool flatSlot)
//        {
//            pageIndex = 0;
//            columnIndex = 0;
//            columnPrefix = "";
//            flatSlot = false;
//            switch (slot.DataType)
//            {
//                case DataType.String:
//                    columnPrefix = SqlProvider.StringMappingPrefix;
//                    pageIndex = slot.Mapping / SqlProvider.StringPageSize;
//                    columnIndex = slot.Mapping % SqlProvider.StringPageSize;
//                    flatSlot = true;
//                    break;
//                case DataType.Int:
//                    columnPrefix = SqlProvider.IntMappingPrefix;
//                    pageIndex = slot.Mapping / SqlProvider.IntPageSize;
//                    columnIndex = slot.Mapping % SqlProvider.IntPageSize;
//                    flatSlot = true;
//                    break;
//                case DataType.Currency:
//                    columnPrefix = SqlProvider.CurrencyMappingPrefix;
//                    pageIndex = slot.Mapping / SqlProvider.CurrencyPageSize;
//                    columnIndex = slot.Mapping % SqlProvider.IntPageSize;
//                    flatSlot = true;
//                    break;
//                case DataType.DateTime:
//                    columnPrefix = SqlProvider.DateTimeMappingPrefix;
//                    pageIndex = slot.Mapping / SqlProvider.DateTimePageSize;
//                    columnIndex = slot.Mapping % SqlProvider.IntPageSize;
//                    flatSlot = true;
//                    break;
//                case DataType.Reference:
//                    columnPrefix = "Reference_";
//                    break;
//                default:
//                    break;
//            }
//        }

//        private static string GetPageEqualString(string alias)
//        {
//            return string.Format(CultureInfo.InvariantCulture, PAGEEQUALPAGEINDEX, alias);
//        }

//        private static string GetExtraTableJoinString(string alias)
//        {
//            return string.Format(CultureInfo.InvariantCulture, EXTRATABLEJOIN, alias);
//        }

//        private MultiPage GetMultiPage(string name)
//        {
//            foreach (MultiPage mp in _multiPageIndex)
//            {
//                if (mp.Name == name)
//                    return mp;
//            }
//            MultiPage newMp = new MultiPage(name);
//            _multiPageIndex.Add(newMp);
//            return newMp;
//        }

//        //===========================================================================================

//        private void LevelHasPropertySlot(int refLevel)
//        {
//            InitializeReferenceLevel(refLevel);
//            _hasPropertyOnLevel[refLevel] = true;
//        }
//        private void LevelHasAttrReference(int refLevel)
//        {
//            InitializeReferenceLevel(refLevel);
//            _hasAttrReferenceOnLevel[refLevel] = true;
//        }
//        private void LevelHasSlotReference(int refLevel)
//        {
//            InitializeReferenceLevel(refLevel);
//            _hasSlotReferenceOnLevel[refLevel] = true;
//        }
//        private void InitializeReferenceLevel(int refLevel)
//        {
//            while (_hasPropertyOnLevel.Count <= refLevel)
//                _hasPropertyOnLevel.Add(false);
//            while (_hasAttrReferenceOnLevel.Count <= refLevel)
//                _hasAttrReferenceOnLevel.Add(false);
//            while (_hasSlotReferenceOnLevel.Count <= refLevel)
//                _hasSlotReferenceOnLevel.Add(false);
//        }

//        private static string GetSlotAlias(List<PropertyLiteral> refChain, string subfix)
//        {
//            int levelIndex = refChain.Count - 1;
//            if (levelIndex == -1)
//                return "ROOT0.";

//            string attr = GetAttr(refChain[0].NodeAttribute);
//            if (attr != "ATTR")
//                return String.Concat(attr, subfix, "_", levelIndex, ".");

//            return String.Concat("REF_", subfix, "_", levelIndex, ".");
//        }
//        private static string PrepareLikeValue(string value, LikeOperator op)
//        {
//            switch (op)
//            {
//                case LikeOperator.StartsWith:
//                    return String.Concat(value, "%");
//                case LikeOperator.Contains:
//                    return String.Concat("%", value, "%");
//                case LikeOperator.EndsWith:
//                    return String.Concat("%", value);
//                default:
//                    return value;
//            }
//        }

//        private string Indent(int indent)
//        {
//            while (indent > _tabs.Length)
//                _tabs += _tabs;
//            return _tabs.Substring(0, indent);
//        }
//    }

//    internal class LevelSwitch
//    {
//        public LevelSwitch(string subfix, bool isSlot)
//        {
//            _subfix = subfix;
//            _isSlot = isSlot;
//        }

//        public LevelSwitch(string subfix)
//        {
//            _subfix = subfix;
//        }

//        private string _subfix = string.Empty;

//        public string Subfix
//        {
//            get { return _subfix; }
//            set { _subfix = value; }
//        }

//        private bool _isSlot;// = false;

//        public bool IsSlot
//        {
//            get { return _isSlot; }
//            set { _isSlot = value; }
//        }

//        private PropertyLiteral _propLit;// = null;

//        public PropertyLiteral PropLit
//        {
//            get { return _propLit; }
//            set { _propLit = value; }
//        }

//    }

//    internal class MultiPage
//    {
//        public MultiPage(string name)
//        {
//            Name = name;
//            IntList = new List<int>();
//        }

//        public List<int> IntList { get; set; }
//        public string Name { get; set; }
//    }
//}
