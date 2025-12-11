using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FinalProjectBleu
{
    public partial class AdminMenuForm : Form
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS;Initial Catalog=FoodOrderingDB;Integrated Security=True;";

        public AdminMenuForm()
        {
            InitializeComponent();
        }

        private void AdminMenuForm_Load(object sender, EventArgs e)
        {
            LoadMenu();
        }

        private void LoadMenu()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT ItemID, ItemName, Category, Price, Status FROM Menu", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvMenu.DataSource = dt;
            }
        }

        private void SearchMenu(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, ItemName, Category, Price, Status FROM Menu WHERE ItemName LIKE @kw OR Category LIKE @kw";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvMenu.DataSource = dt;
            }
        }

        private void ToggleItemAvailability(int itemId, string currentStatus)
        {
            string newStatus = currentStatus.Equals("Available", StringComparison.OrdinalIgnoreCase) ? "Not Available" : "Available";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Menu SET Status=@newStatus WHERE ItemID=@id", conn);
                    cmd.Parameters.AddWithValue("@newStatus", newStatus);
                    cmd.Parameters.AddWithValue("@id", itemId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"Item status successfully toggled to {newStatus} (ID: {itemId}).");

                    LoadMenu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error during toggle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvMenu.CurrentRow == null)
            {
                MessageBox.Show("Please select an item to toggle its availability.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvMenu.CurrentRow.Cells["ItemID"].Value);
            string currentStatus = dgvMenu.CurrentRow.Cells["Status"].Value.ToString();

            string nextStatus = currentStatus.Equals("Available", StringComparison.OrdinalIgnoreCase) ? "Not Available" : "Available";

            DialogResult result = MessageBox.Show(
                $"Toggle status for Item ID {id} ({dgvMenu.CurrentRow.Cells["ItemName"].Value})? \n\n" +
                $"Current Status: {currentStatus}\n" +
                $"New Status will be: {nextStatus}",
                "Confirm Status Toggle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ToggleItemAvailability(id, currentStatus);
            }
        }


        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Menu (ItemName, Category, Price) VALUES (@n, @c, @p)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@n", txtItemName.Text);
                    cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@p", Convert.ToDecimal(txtPrice.Text));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Item added successfully!");
                    LoadMenu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (dgvMenu.CurrentRow == null) return;
            int id = Convert.ToInt32(dgvMenu.CurrentRow.Cells["ItemID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Menu SET ItemName=@n, Category=@c, Price=@p WHERE ItemID=@id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@n", txtItemName.Text);
                    cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@p", Convert.ToDecimal(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Item updated!");
                    LoadMenu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            new AdminDashboard().Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
                LoadMenu();
            else
                SearchMenu(keyword);
        }
       
        private void HardDeleteItem(int itemId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Menu WHERE ItemID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", itemId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Item ID {itemId} was permanently deleted. Historical order items referencing this ID were set to NULL.", "Hard Delete Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMenu(); 
                    }
                    else
                    {
                        MessageBox.Show("Could not find item to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during hard delete: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHardDelete_Click(object sender, EventArgs e)
        {
            if (dgvMenu.CurrentRow == null)
            {
                MessageBox.Show("Please select an item to permanently delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvMenu.CurrentRow.Cells["ItemID"].Value);
            string itemName = dgvMenu.CurrentRow.Cells["ItemName"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"WARNING: Are you sure you want to PERMANENTLY delete '{itemName}' (ID: {id})?\n\n" +
                "This item will be removed from the menu and replaced with NULL in all historical orders.",
                "CONFIRM PERMANENT DELETE",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            if (result == DialogResult.Yes)
            {
                HardDeleteItem(id);
            }
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgvMenu.CurrentRow != null && dgvMenu.CurrentRow.Index >= 0)
            {
                DataGridViewRow row = dgvMenu.CurrentRow;

                if (dgvMenu.Columns.Contains("ItemName"))
                    txtItemName.Text = row.Cells["ItemName"].Value?.ToString() ?? "";

                if (dgvMenu.Columns.Contains("Price"))
                    txtPrice.Text = row.Cells["Price"].Value?.ToString() ?? "";

                if (dgvMenu.Columns.Contains("Category"))
                    txtCategory.Text = row.Cells["Category"].Value?.ToString() ?? "";
            }
        }
    }
}