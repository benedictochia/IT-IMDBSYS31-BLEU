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
    public partial class AdminDashboard : Form
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {

        }
        private void btnManageMenu_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new AdminMenuForm().Show();
        }

        private void btnViewOrders_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new AdminOrdersForm().Show();
        }
        
        private void btnLogout_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new AdminLoginForm().Show();
        }
    }
}
