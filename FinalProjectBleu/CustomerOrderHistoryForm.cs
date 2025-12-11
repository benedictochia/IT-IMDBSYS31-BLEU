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
    public partial class CustomerOrderHistoryForm : Form
    {
        private int customerID;
        private int currentCustomerID;
        private string connectionString = "Server=localhost\\SQLEXPRESS;Initial Catalog=FoodOrderingDB;Integrated Security=True;";

        public CustomerOrderHistoryForm(int customerID)
        {
            InitializeComponent();
            this.customerID = customerID;
            currentCustomerID = customerID;
        }

        private void CustomerOrderHistoryForm_Load(object sender, EventArgs e)
        {
            LoadOrderHistory();
            dgvOrderHistory.ReadOnly = true;
            if (dgvOrderDetails != null)
            {
                dgvOrderDetails.ReadOnly = true;
            }
        }

        private void LoadOrderHistory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT OrderID, OrderDate, TotalAmount, Status FROM Orders WHERE CustomerID = @id ORDER BY OrderDate DESC";


                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", customerID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvOrderHistory.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order history: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderDetails(int orderId)
        {
            if (dgvOrderDetails == null) return; 

            string query = @"
                SELECT 
                    m.ItemName, 
                    oi.Quantity, 
                    m.Price,        -- Price per unit from the Menu
                    oi.Subtotal     -- Calculated subtotal (Quantity * Price)
                FROM 
                    OrderItems oi
                INNER JOIN 
                    Menu m ON oi.ItemID = m.ItemID
                WHERE 
                    oi.OrderID = @orderId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@orderId", orderId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvOrderDetails.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dgvOrderHistory_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgvOrderHistory.CurrentRow == null || dgvOrderHistory.CurrentRow.Index < 0)
            {
                if (dgvOrderDetails?.DataSource is DataTable dt)
                {
                    dt.Clear();
                }
                return;
            }

            DataGridViewRow row = dgvOrderHistory.CurrentRow;
            object orderIdValue = null;

            if (dgvOrderHistory.Columns.Contains("OrderID"))
            {
                orderIdValue = row.Cells["OrderID"].Value;
            }
            if (orderIdValue == null)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn.HeaderText.Equals("OrderID", StringComparison.OrdinalIgnoreCase))
                    {
                        orderIdValue = cell.Value;
                        break;
                    }
                }
            }

            if (orderIdValue != null)
            {
                if (int.TryParse(orderIdValue.ToString(), out int selectedOrderId))
                {
                    LoadOrderDetails(selectedOrderId);
                }
            }
            else
            {
                if (dgvOrderDetails?.DataSource is DataTable dt)
                {
                    dt.Clear();
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            new CustomerDashboard(currentCustomerID).Show();
        }
    }
}