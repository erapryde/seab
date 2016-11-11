using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SeabBot.Utility
{
    public class Organisation
    {
        private Dictionary<string, Division> dicDivisions = new Dictionary<string, Division>();
        private Division mMainDiv;
        private Department mMainDep;
        public string ID { get; private set; }
        public string Name { get; private set; }

        private Organisation()
        {
            mMainDiv = new Division(this, "-", "-");
            mMainDep = mMainDiv.GetDepartment("-");
        }
        public Organisation(string id, string name) : this()
        {
            ID = id;
            Name = name;
        }
        private static XmlNode GetOrgNode(XmlDocument doc)
        {
            foreach(XmlNode nd in doc.ChildNodes)
            {
                if (nd.NodeType == XmlNodeType.Element)
                    if (string.Compare(nd.Name, "Organisation", true) == 0) return nd;
            }
            return null;
        }
        private static XmlDocument GetDocumentFromXmlText(string xmlText)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            return doc;
        }

        public Organisation(string xmlText) : this(GetDocumentFromXmlText(xmlText))
        {

        }
        public Organisation(XmlDocument doc):this(GetOrgNode(doc))
        {

        }
        public Organisation(XmlNode nd) : this()
        {
            Name = nd.Attributes["Name"].Value;
            ID = nd.Attributes["ID"].Value;

            foreach (XmlNode child in nd.ChildNodes)
            {
                
                
                switch (child.Name.ToUpper())
                {
                    case "PERSON":
                        Person p = new Person(mMainDep, child);
                        break;
                    case "DIVISION":
                        Division div = new Division(this, child);
                        dicDivisions.Add(div.ID, div);
                        break;
                    case "#COMMENT":
                        break;
                    default:
                        throw new Exception("Unexpected Tag Name " + child.Name + " in Organisation Node");
                }
                 
            }
        }


        /// <summary>
        /// Returns Persons who are at the top of the organisation
        /// </summary>
        public System.Collections.Generic.IEnumerable<Person> Persons
        {
            get
            {
                foreach (Person p in mMainDiv.Persons)
                    yield return p;

            }

        }

        /// <summary>
        /// Iterates through all Divisons in the organisation
        /// </summary>
        public System.Collections.Generic.IEnumerable<Division> Divisions
        {
            get
            {
                foreach (Division div in dicDivisions.Values)
                    yield return div;

            }

        }

        /// <summary>
        /// Iterates through all Persons in the organisation
        /// </summary>
        public System.Collections.Generic.IEnumerable<Person> AllPersons
        {
            get
            {
                foreach (Person p in Persons)
                    yield return p;

                foreach (var div in dicDivisions.Values)
                    foreach (Person p in div.AllPersons)
                        yield return p;

            }
        }


        private int Index(List<string> lst, string match)
        {
            int index = 0;
            foreach (string d in lst)
            {
                if (string.Compare(d, match, true) == 0) return index;
                index++;
            }
            return -1;
        }


        /// <summary>
        /// Returns the Division with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Division Division(string id)
        {
            if (id == "" || id == "-") return mMainDiv;
            return dicDivisions[id];

        }


        public void AddDivisionName(string ID, string longName)
        {
            Division div = new Division(this, ID, longName);
            dicDivisions.Add(ID, div);

        }
        public String FindByName(string divName)
        {
            if (divName == "" || divName == null || divName == "-") return "-";
            foreach (Division div in dicDivisions.Values)
            {
                if (string.Compare(div.Name, divName, true) == 0) return div.ID;
            }
            throw new Exception("Cannot find Division ByName: " + divName);

        }

        public XmlDocument XmlDocument
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                XmlNode ndOrg = doc.CreateNode(XmlNodeType.Element, "Organisation", null);

                ndOrg.AddAttribute("ID", ID);
                ndOrg.AddAttribute("Name", Name);
                foreach (Person p in mMainDiv.Persons)
                {
                    p.AddNode(ndOrg);
                }

                foreach (var d in this.dicDivisions)
                {
                    d.Value.AddNode(ndOrg);


                }
                doc.AppendChild(ndOrg);
                return doc;
            }
        }


    }

    public class Division
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        private List<Department> lstDepartment = new List<Department>();
        private Department mMainDep;

        public Organisation Organisation { get; private set; }

        private Division(Organisation org)
        {
            Organisation = org;
            mMainDep = new Department(this, "-");
        }
        public Division(Organisation org, string id, string name) : this(org)
        {

            Name = name;
            ID = id;


        }

        public Division(Organisation org, XmlNode nd) : this(org)
        {
            Name = nd.Attributes["Name"].Value;
            ID = nd.Attributes["ID"].Value;

            foreach (XmlNode child in nd.ChildNodes)
            {
                switch (child.Name.ToUpper())
                {
                    case "PERSON":
                        Person p = new Person(mMainDep, child);
                        break;
                    case "DEPARTMENT":
                        Department dep = new Department(this, child);
                        lstDepartment.Add(dep);
                        break;
                    case "#COMMENT":
                        break;
                    default:
                        throw new Exception("Unexpected Tag Name " + child.Name + " in Division Node");
                }
            }
        }

        /// <summary>
        /// Iterates through the top level Persons of the Division (e.g. Directors, Deputies, PA etc)
        /// </summary>
        public System.Collections.Generic.IEnumerable<Person> Persons
        {
            get
            {
                foreach (Person p in mMainDep.Persons)
                    yield return p;


            }

        }

        /// <summary>
        /// Iterates through all the people in this division
        /// </summary>
        public System.Collections.Generic.IEnumerable<Person> AllPersons
        {
            get
            {
                foreach (Person p in mMainDep.Persons)
                    yield return p;

                foreach (var depa in lstDepartment)
                    foreach (Person p in depa.Persons)
                        yield return p;

            }
        }

        /// <summary>
        /// Iterates through the departments in this division
        /// </summary>
        public System.Collections.Generic.IEnumerable<Department> Departments
        {
            get
            {
                foreach (Department dep in lstDepartment)
                    yield return dep;

            }

        }

        /// <summary>
        /// Gets the department by name creating it if necessary
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Department GetDepartment(string name)
        {
            if (name == "" || name == "-") return mMainDep;
            foreach (Department depa in lstDepartment)
            {
                if (string.Compare(name, depa.Name, true) == 0) return depa;
            }
            Department dep = new Department(this, name);
            lstDepartment.Add(dep);
            return dep;

        }


        public void AddNode(XmlNode parent)
        {
            XmlNode nd = parent.OwnerDocument.CreateNode(XmlNodeType.Element, "Division", null);
            nd.AddAttribute("Name", Name);
            nd.AddAttribute("ID", ID);

            foreach (Person p in mMainDep.Persons)
            {
                p.AddNode(nd);
            }
            foreach (Department depa in lstDepartment)
            {
                depa.AddNode(nd);
            }
            parent.AppendChild(nd);

        }

    }

    public class Department
    {
        public string Name { get; private set; }
        public Division Division { get; private set; }
        private List<Person> lstPersons = new List<Person>();

        public Department(Division div, string name)
        {
            Division = div;
            Name = name;
        }

        public Department(Division div, XmlNode nd)
        {
            Division = div;
            Name = nd.Attributes["Name"].Value;

            foreach (XmlNode child in nd.ChildNodes)
            {
                if (string.Compare(child.Name, "Person", true) == 0)
                {
                    Person p = new Person(this, child);
                    //  lstPersons.Add(p);


                }
                else if (string.Compare(child.Name, "#Comment", true)!=0) throw new Exception("Unexpected Tag Name " + child.Name);

            }


        }

        internal void Add(Person p)
        {
            if (lstPersons.Contains(p)) return;
            p.Department = this;
            lstPersons.Add(p);
        }

        public System.Collections.Generic.IEnumerable<Person> Persons
        {
            get
            {
                foreach (Person p in lstPersons)
                    yield return p;


            }

        }


        public void AddNode(XmlNode parent)
        {
            XmlNode nd = parent.OwnerDocument.CreateNode(XmlNodeType.Element, "Department", null);
            nd.AddAttribute("Name", Name);

            foreach (Person p in lstPersons)
            {
                p.AddNode(nd);
            }
            parent.AppendChild(nd);


        }


    }

    public class Person
    {
        public string Telephone { get; private set; }
        public string Location { get; private set; }
        public string Name { get; private set; }
        public string Image { get; private set; }
        public string Designation { get; private set; }

        public string Sex { get; private set; }

        public Department Department;



        public Person(Department dep, string nm, string sex, string des, string loc, string tel, string im)
        {
            Name = nm;
            Sex = sex;
            Designation = des;
            Location = loc;
            Telephone = tel;
            Image = im;
            dep.Add(this);

        }
         
        public bool Contains(string a)
        {
            if (NameContains(a)) return true;
            if (DesignationContains(a)) return true;
            if (LocationContains(a)) return true;
            if (SexIs(a)) return true;
            if (TelephoneContains(a)) return true;
            return false;
        }
         
        private string ConvertSex(string a)
        {
            a = a.ToUpper();
            if (a == "M" || a == "F") return a;
            if (a == "BOY" || a == "MALE") return "M";
            if (a == "GIRL" || a == "FEMALE") return "F";
            return "?";

        }
        public bool SexIs(string a)
        {
            return Sex == ConvertSex(a);
        }
        public bool NameContains(string a)
        {
            return Name.IndexOf(a, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        public bool DesignationContains(string a)
        {
            return Designation.IndexOf(a, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        public bool TelephoneContains(string a)
        {
            return Telephone.IndexOf(a, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        public bool LocationContains(string a)
        {
            return Location.IndexOf(a, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public Person(Department dep, XmlNode nd)
        {
            this.Department = dep;
            Name = GetChildNodeText(nd, "Name");
            Sex = GetChildNodeText(nd, "Sex");
            Designation = GetChildNodeText(nd, "Des");
            Location = GetChildNodeText(nd, "Loc");
            Telephone = GetChildNodeText(nd, "Tel");
            Image = GetChildNodeText(nd, "Im");
            dep.Add(this);
        }

        private string GetChildNodeText(XmlNode parent, string name)
        {
            try
            {
                return parent[name].InnerText;
            }
            catch
            {
                return "";
            }
        }

        public override bool Equals(object obj)
        {
            Person ps = obj as Person;
            if (ps == null) return false;
            if (ps.Name != Name) return false;
            if (ps.Location != Location) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int v = Name != null ? Name.GetHashCode() : 0;
            v ^= Location != null ? Location.GetHashCode() : 0;
            return v;
        }
        public Division Division
        {
            get { return this.Department.Division; }
        }

        public Organisation Organisation
        {
            get { return this.Department.Division.Organisation; }
        }


        public void AddNode(XmlNode parent)
        {
            XmlNode nd = parent.OwnerDocument.CreateNode(XmlNodeType.Element, "Department", null);
            XmlNode ndPerson = parent.OwnerDocument.CreateNode(XmlNodeType.Element, "Person", null);
            parent.AppendChild(ndPerson);
            AddNode(ndPerson, "Name", Name);
            AddNode(ndPerson, "Sex", Sex);
            AddNode(ndPerson, "Des", Designation);
            AddNode(ndPerson, "Loc", Location);
            AddNode(ndPerson, "Tel", Telephone);
            AddNode(ndPerson, "Im", Image);
        }


        private XmlNode AddNode(XmlNode parent, string name, string contents)
        {
            XmlNode nd = parent.OwnerDocument.CreateNode(XmlNodeType.Element, name, null);
            nd.InnerText = contents;
            parent.AppendChild(nd);
            return nd;

        }









    }
    public static class EXT
    {
        public static void AddAttribute<T>(this XmlNode nd, string name, T value)
        {
            XmlAttribute att = null;
            XmlDocument doc = nd.OwnerDocument;
            att = (XmlAttribute)doc.CreateNode(XmlNodeType.Attribute, name, null);
            att.Value = value.ToString();
            nd.Attributes.Append(att);
        }




    }
}
