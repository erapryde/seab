using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Organisation
{
    
    public partial class Form1 : Form
    {
        private ORG.Organisation organisation;
        public Form1()
        {
            InitializeComponent();
            organisation = new ORG.Organisation(Organisation.Properties.Resources.ORGXml);
            
            foreach (var v in organisation.AllPersons)
                if (v.SexIs("Female")) Console.WriteLine(v.Name + " " + v.Designation);



            
            
        }
    }
}
