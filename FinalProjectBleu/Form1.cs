using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FinalProjectBleu
{
    public partial class AdminLoginForm : Form
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS;Initial Catalog=FoodOrderingDB;Integrated Security=True;";
        public static int LoggedCustomerID;

        public AdminLoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string adminQuery = "SELECT * FROM Admins WHERE Username=@u AND Password=@p";
                SqlCommand adminCmd = new SqlCommand(adminQuery, conn);
                adminCmd.Parameters.AddWithValue("@u", username);
                adminCmd.Parameters.AddWithValue("@p", password);

                SqlDataReader dr = adminCmd.ExecuteReader();
                if (dr.Read())
                {
                    dr.Close();
                    this.Hide();
                    new AdminDashboard().Show();
                    return;
                }
                dr.Close();
                string customerQuery = "SELECT CustomerID FROM Customers WHERE Username=@u AND Password=@p";
                SqlCommand customerCmd = new SqlCommand(customerQuery, conn);
                customerCmd.Parameters.AddWithValue("@u", username);
                customerCmd.Parameters.AddWithValue("@p", password);

                SqlDataReader dr2 = customerCmd.ExecuteReader();
                if (dr2.Read())
                {
                    int customerID = (int)dr2["CustomerID"];
                    dr2.Close();
                    this.Hide();
                    new CustomerDashboard(customerID).Show();
                    return;
                }
                dr2.Close();
                lblMessage.Text = "Invalid username or password.";
            }
        }

        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new RegisterForm().Show();
        }

        private void AdminLoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
