using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProjectBleu
{
    public partial class RegisterForm : Form
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS;Initial Catalog=FoodOrderingDB;Integrated Security=True;";
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

               
                string checkUser = "SELECT COUNT(*) FROM Customers WHERE Username = @u";
                SqlCommand checkCmd = new SqlCommand(checkUser, conn);
                checkCmd.Parameters.AddWithValue("@u", username);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    lblMessage.Text = "Username already exists!";
                    return;
                }

               
                string query = "INSERT INTO Customers (Username, Password) VALUES (@u, @p)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.ExecuteNonQuery();

                lblMessage.Text = "Registration successful!";
                txtUsername.Clear();
                txtPassword.Clear();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            new AdminLoginForm().Show();
        }
    }
}
