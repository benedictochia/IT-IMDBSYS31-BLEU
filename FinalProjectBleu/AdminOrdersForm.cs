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
    public partial class AdminOrdersForm : Form
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS;Initial Catalog=FoodOrderingDB;Integrated Security=True;";

        public AdminOrdersForm()
        {
            InitializeComponent();
        }

        private void AdminOrdersForm_Load(object sender, EventArgs e)
        {
            LoadPendingOrders();
            LoadCompletedOrders();

            if (dgvPendingOrders != null) dgvPendingOrders.ReadOnly = true;
            if (dgvCompletedOrders != null) dgvCompletedOrders.ReadOnly = true;
            if (dgvOrderDetails != null) dgvOrderDetails.ReadOnly = true;
            dgvPendingOrders.CurrentCellChanged += dgvOrders_CurrentCellChanged;
            dgvCompletedOrders.CurrentCellChanged += dgvOrders_CurrentCellChanged;
        }

        private void LoadPendingOrders()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT o.OrderID, c.Username AS Customer, o.OrderDate, o.TotalAmount, o.Status
                    FROM Orders o
                    INNER JOIN Customers c ON o.CustomerID = c.CustomerID
                    WHERE o.Status = 'Pending'
                    ORDER BY o.OrderDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvPendingOrders.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pending orders: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCompletedOrders()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT o.OrderID, c.Username AS Customer, o.OrderDate, o.TotalAmount, o.Status
                    FROM Orders o
                    INNER JOIN Customers c ON o.CustomerID = c.CustomerID
                    WHERE o.Status = 'Completed'
                    ORDER BY o.OrderDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvCompletedOrders.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading completed orders: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void dgvOrders_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (dgv == null || dgv.CurrentRow == null || dgv.CurrentRow.Index < 0)
            {
                if (dgvOrderDetails?.DataSource is DataTable dt)
                {
                    dt.Clear();
                }
                return;
            }

            if (dgv.Name == dgvPendingOrders.Name && dgvCompletedOrders.CurrentRow != null)
            {
                dgvCompletedOrders.ClearSelection();
            }
            else if (dgv.Name == dgvCompletedOrders.Name && dgvPendingOrders.CurrentRow != null)
            {
                dgvPendingOrders.ClearSelection();
            }


            DataGridViewRow row = dgv.CurrentRow;
            object orderIdValue = null;

            if (dgv.Columns.Contains("OrderID"))
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

            if (orderIdValue != null && int.TryParse(orderIdValue.ToString(), out int selectedOrderId))
            {
                LoadOrderDetails(selectedOrderId);
            }
            else
            {
                if (dgvOrderDetails?.DataSource is DataTable dt)
                {
                    dt.Clear();
                }
            }
        }

        private void ToggleOrderStatus(int orderId, string currentStatus)
        {
            string newStatus = currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ? "Pending" : "Completed";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Orders SET Status=@newStatus WHERE OrderID=@id", conn);
                    cmd.Parameters.AddWithValue("@newStatus", newStatus);
                    cmd.Parameters.AddWithValue("@id", orderId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"Order {orderId} status successfully changed from {currentStatus} to {newStatus}.", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadPendingOrders();
                    LoadCompletedOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error during status update: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateStatus_Click_1(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = null;
            DataGridView dgvSource = null;

            if (dgvPendingOrders.CurrentRow != null)
            {
                selectedRow = dgvPendingOrders.CurrentRow;
                dgvSource = dgvPendingOrders;
            }
            else if (dgvCompletedOrders.CurrentRow != null)
            {
                selectedRow = dgvCompletedOrders.CurrentRow;
                dgvSource = dgvCompletedOrders;
            }

            if (selectedRow == null)
            {
                MessageBox.Show("Please select an order to update its status from either the Pending or Completed lists.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            object orderIdValue = null;
            if (dgvSource.Columns.Contains("OrderID"))
            {
                orderIdValue = selectedRow.Cells["OrderID"].Value;
            }

            if (orderIdValue == null || !int.TryParse(orderIdValue.ToString(), out int orderId))
            {
                MessageBox.Show("Could not find or parse Order ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string currentStatus = selectedRow.Cells["Status"].Value.ToString();
            string nextStatus = currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ? "Pending" : "Completed";

            DialogResult result = MessageBox.Show(
                $"Toggle status for Order ID {orderId}? \n\n" +
                $"Current Status: {currentStatus}\n" +
                $"New Status will be: {nextStatus}",
                "Confirm Status Toggle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ToggleOrderStatus(orderId, currentStatus);
            }
        }


        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            LoadPendingOrders();
            LoadCompletedOrders();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            new AdminDashboard().Show();
        }
    }
}