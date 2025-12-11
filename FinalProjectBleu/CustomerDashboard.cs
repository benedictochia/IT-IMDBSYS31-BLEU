using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectBleu
{
    public partial class CustomerDashboard : Form
    {
        private int customerID;
        public CustomerDashboard(int customerID)
        {
            InitializeComponent();
            this.customerID = customerID;
        }


        private void CustomerDashboard_Load(object sender, EventArgs e)
        {

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            new AdminLoginForm().Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.Hide();
            new CustomerOrderForm(customerID).Show();
        }

        private void btnHistory_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new CustomerOrderHistoryForm(customerID).Show();
        }
    }
}
