﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace WindowsFormsApplication1
{
    public partial class EmpMgtInfo : UserControl
    {
        OleDbConnection cn = new OleDbConnection();
        OleDbCommand cmd = new OleDbCommand();
        OleDbDataAdapter da = new OleDbDataAdapter();
        DataTable dt = new DataTable();
        DataSet ds = new DataSet();

        public EmpMgtInfo()
        {
            InitializeComponent();
        }

        private DataTable CreateSchemaDataTable(OleDbDataReader reader)
        {
            if (reader == null) { return null; }
            if (reader.IsClosed) { return null; }

            DataTable schema = reader.GetSchemaTable();
            DataTable dt = new DataTable();

            foreach (DataRow row in schema.Rows)
            {
                // Column情報を追加してください。
                DataColumn col = new DataColumn();
                col.ColumnName = row["ColumnName"].ToString();
                col.DataType = Type.GetType(row["DataType"].ToString());

                if (col.DataType.Equals(typeof(string)))
                {
                    col.MaxLength = (int)row["ColumnSize"];
                }

                dt.Columns.Add(col);
            }
            return dt;
        }

        private void SearchB_Click(object sender, EventArgs e)
        {
            da = new OleDbDataAdapter("SELECT 従業員ID,パスワード,名前,ﾌﾘｶﾞﾅ,性別,郵便番号,住所1,住所2,電話番号,生年月日,入社日,責任者権限 FROM 従業員マスタ" +
            " WHERE 従業員ID LIKE '%" + EmpIDTextB.Text + "%'" +
            " AND 名前 LIKE '%" + EmpNameTextB.Text + "%'" +
            " AND フラグ <> '退職'", cn);
            dt = new DataTable();
            da.Fill(dt);

            EmpdataGridView.DataSource = dt;

            EmpIDTextB.Text = "";
            EmpNameTextB.Text = "";
        }

        private void EditB_Click(object sender, EventArgs e)
        {
            if (EmpdataGridView.ReadOnly == true)
            {
                EmpdataGridView.ReadOnly = false;
                EmpdataGridView.Columns[0].ReadOnly = true;
                EmpdataGridView.Columns[10].ReadOnly = true;
                MessageBox.Show("編集可能になりました。", "編集可能");
                EditLbl.Text = "編集中";
                DeleteB.Visible = true;
                UpdateB.Visible = true;
            }
            else if (EmpdataGridView.ReadOnly == false)
            {
                EmpdataGridView.ReadOnly = true;
                MessageBox.Show("編集不可になりました。", "編集不可");
                EditLbl.Text = "";
                DeleteB.Visible = false;
                UpdateB.Visible = false;
            }
        }

        private void DeleteB_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("削除しますか？", "OICSystem", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                cn.Open();
                cmd.Connection = cn;
                cmd.CommandText = "UPDATE 従業員マスタ SET フラグ = '退職' WHERE 名前='" + EmpNameTextB.Text + "'";
                cmd.ExecuteNonQuery();
                try
                {

                    MessageBox.Show("削除しました", "OICSystem");


                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.Message, "IM2");
                }
                cn.Close();
                dataload(0);
                EmpIDTextB.Text = "";
                EmpNameTextB.Text = "";
            }
            else if (result == DialogResult.No)
            {

            }
        }

        private void UpdateB_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < EmpdataGridView.Rows.Count; i++)
            {
                cmd.Connection = cn;

                cmd.CommandText =
                    "UPDATE 従業員マスタ SET パスワード ='" + EmpdataGridView[1, i].Value.ToString() +
                    "' ,名前 ='" + EmpdataGridView[2, i].Value.ToString() +
                    "' ,ﾌﾘｶﾞﾅ ='" + EmpdataGridView[3, i].Value.ToString() +
                    "' ,性別 ='" + EmpdataGridView[4, i].Value.ToString() +
                    "' ,郵便番号 ='" + EmpdataGridView[5, i].Value.ToString() +
                    "' ,住所1 ='" + EmpdataGridView[6, i].Value.ToString() +
                    "' ,住所2 ='" + EmpdataGridView[7, i].Value.ToString() +
                    "' ,電話番号 ='" + EmpdataGridView[8, i].Value.ToString() +
                    "' ,責任者権限 =" + EmpdataGridView[11, i].Value +
                    " WHERE 従業員ID=" + (int)EmpdataGridView[0, i].Value;
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();

            }
            MessageBox.Show("更新しました", "OICSystem");

        }
        private void EmpdataGridView_Click(object sender, EventArgs e)
        {
            EmpIDTextB.Text = (string)EmpdataGridView.CurrentRow.Cells[0].Value.ToString();
            EmpNameTextB.Text = (string)EmpdataGridView.CurrentRow.Cells[2].Value.ToString();
        }
        private void dataload(int n)
        {
            EmpdataGridView.Columns.Clear();
            EmpdataGridView.DataSource = null;
            cn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" + @"Data Source=.\DB\IM2.accdb;";
            da = new OleDbDataAdapter("SELECT 従業員ID,パスワード,名前,ﾌﾘｶﾞﾅ,性別,郵便番号,住所1,住所2,電話番号,生年月日,入社日,責任者権限 FROM 従業員マスタ WHERE フラグ <> '退職'", cn);
            dt.Clear();
            dt = new DataTable();
            da.Fill(dt);
            EmpdataGridView.DataSource = dt;
            EmpdataGridView.AutoResizeColumns();
            EmpdataGridView.ClearSelection();
            EmpdataGridView.Rows[n].Selected = true;
            EmpdataGridView.FirstDisplayedScrollingRowIndex = n;
            foreach (DataGridViewColumn c in EmpdataGridView.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void EmpMgtInfo_Load(object sender, EventArgs e)
        {
            dataload(0);
            EmpdataGridView.AllowUserToAddRows = false;
        }

        private void selectfunc(string cmdstr)
        {
            dt.Clear();
            dt = new DataTable();
            EmpdataGridView.DataSource = null;
            da = new OleDbDataAdapter(cmdstr, cn);
            da.Fill(dt);
            EmpdataGridView.DataSource = dt;
            EmpdataGridView.AutoResizeColumns();
        }

    }
}
