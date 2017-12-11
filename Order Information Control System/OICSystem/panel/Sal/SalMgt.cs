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
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
	public partial class SalMgt : UserControl
	{

		OleDbCommand cmd = new OleDbCommand();
		DataSet sal = new DataSet();
		
		
		string ordercode = "";

		public SalMgt()
		{
			InitializeComponent();
		}


		protected DataTable GetDataOrder()
		{
			OleDbConnection cn = new OleDbConnection("Provider=microsoft.ace.oledb.12.0;" + @"Data Source=.\DB\IM2.accdb;");

			string DSCheck = DateStart.Text;
			string DECheck = DateEnd.Text;

			DateTime start = new DateTime(1500, 1, 1); ;

			DataTable dt = new DataTable();
			ordercode = "";

			try
			{
				if (DateStart.Text == "    /  /" && DateEnd.Text == "    /  /")     //日付指定　＃-'
				{
					OleDbDataAdapter da = new OleDbDataAdapter("SELECT 注文日,商品ID FROM 注文テーブル WHERE 入金済み in (true) ORDER BY 注文日", cn);
					Msg.Text = "全てのデータ";
					da.Fill(dt);
				}
				else if (DateEnd.Text == "    /  /")
				{
					OleDbDataAdapter da = new OleDbDataAdapter("SELECT 注文日,商品ID FROM 注文テーブル WHERE 注文日 Between #" + DateStart.Text + "#  AND 入金済み in (true) ORDER BY 注文日", cn);
					Msg.Text = DSCheck + "のデータ";
					da.Fill(dt);
				}
				else if (DateStart.Text == "    /  /")
				{
					OleDbDataAdapter da = new OleDbDataAdapter("SELECT 注文日,商品ID FROM 注文テーブル WHERE 注文日 Between #" + DateEnd.Text + "# AND 入金済み in (true) ORDER BY 注文日", cn);
					Msg.Text = DECheck + "のデータ";
					da.Fill(dt);
				}
				else
				{
					OleDbDataAdapter da = new OleDbDataAdapter("SELECT 注文日,商品ID FROM 注文テーブル WHERE 注文日 Between #" + DateStart.Text + "# and #" + DateEnd.Text + "# AND 入金済み in (true) ORDER BY 注文日", cn);
					Msg.Text = DSCheck + "～" + DECheck + "のデータ";
					da.Fill(dt);
				}
			}

			catch (Exception )
			{
				Msg.Text = "";
				this.chart1.Visible = false;
				MessageBox.Show("入力された日付は存在しません", "入力エラー");
				
				return null;
			}


			

			DataTable order = new DataTable();   //データテーブルオブジェクトを作成
			DataRow dtRow;



			order.Columns.Add("注文日", Type.GetType("System.DateTime"));
			order.Columns.Add("商品ID", Type.GetType("System.String"));

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				dtRow = order.NewRow();
				dtRow["注文日"] = dt.Rows[i][0];      //DBから取得したdtの注文日を示す行の行番号と列番号
				ordercode += dt.Rows[i][1];
				order.Rows.Add(dtRow);
			}

			return dt;    //データテーブルを返す
		}

		//数量
		private void DateSelectCount_Click(object sender, EventArgs e)
		{
			chart1.Series.Clear();
			DataTable order = GetDataOrder();
			DataSet sal = new DataSet();

			//Goodsテーブルから商品ID,商品名,単価を商品ID順に取り出す
			OleDbConnection cn = new OleDbConnection("Provider=microsoft.ace.oledb.12.0;" + @"Data Source=.\DB\IM2.accdb;");
			OleDbDataAdapter da = new OleDbDataAdapter("SELECT 商品ID,商品名 FROM 商品マスタ ORDER BY 商品ID", cn);
			DataTable dt = new DataTable();
			da.Fill(dt);

			DataTable goods = sal.Tables.Add("goods");
			DataRow dtRow;

			var count = new int[dt.Rows.Count];
			int total;

			goods.Columns.Add("商品ID", Type.GetType("System.String"));
			goods.Columns.Add("商品名", Type.GetType("System.String"));
			goods.Columns.Add("数量", Type.GetType("System.Double"));
			

			//配列の初期化
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				count[i] = 0;
			}
			//数量計算
			for (int i = 0; i < ordercode.Length; i++)
			{
				if (ordercode.Substring(i, 1) != ",")
				{
					count[int.Parse(ordercode.Substring(i, 1)) - 1] += 1;
				}
			}

			//合計金額を初期化
			total = 0;

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				dtRow = goods.NewRow();
				dtRow["商品ID"] = dt.Rows[i][0];      //DBから取得したdtの商品IDを示す行の行番号と列番号
				dtRow["商品名"] = dt.Rows[i][1];  //DBから取得したdtの商品名を示す行の行番号と列番号
				dtRow["数量"] = count[i];
				total += count[i];
				goods.Rows.Add(dtRow);
			}

			Console.WriteLine(count[0]);

			//dataGridView1.DataSource = sal;
			//dataGridView1.DataMember = "goods";

			chart1.Series.Clear();
			chart1.Titles.Clear();
			chart1.Titles.Add("商品別の売上数");
			chart1.Series.Add("数量");


			chart1.Series["数量"].ChartType = SeriesChartType.Bar;       //Bar
			chart1.Series["数量"].IsValueShownAsLabel = true;
			chart1.ChartAreas["ChartArea1"].AxisY.Interval = 1;

			chart1.DataSource = goods;         //チャートに表示するデータテーブルを設定
			chart1.Series["数量"].XValueMember = goods.Columns["商品名"].ColumnName;
			chart1.Series["数量"].YValueMembers = goods.Columns["数量"].ColumnName;
			chart1.DataBind();   //データバインド
			totalMsg.Text = "販売合計数："+total.ToString();
		}

		//金額
		private void DateSelectMoney_Click(object sender, EventArgs e)
		{
			chart1.Series.Clear();
			DataTable order = GetDataOrder();
			DataSet sal = new DataSet();

			//Goodsテーブルから商品ID,商品名,単価を商品ID順に取り出す
			OleDbConnection cn = new OleDbConnection("Provider=microsoft.ace.oledb.12.0;" + @"Data Source=.\DB\IM2.accdb;");
			OleDbDataAdapter da = new OleDbDataAdapter("SELECT 商品ID,商品名,単価 FROM 商品マスタ ORDER BY 商品ID", cn);
			DataTable dt = new DataTable();
			da.Fill(dt);

			DataTable goods = sal.Tables.Add("goods");
			DataRow dtRow;

			var count = new int[dt.Rows.Count];
			string price;
			int sum;
			int total;

			goods.Columns.Add("商品ID", Type.GetType("System.String"));
			goods.Columns.Add("商品名", Type.GetType("System.String"));
			goods.Columns.Add("単価", Type.GetType("System.Double"));
			goods.Columns.Add("数量", Type.GetType("System.Double"));
			goods.Columns.Add("売上", Type.GetType("System.Double"));


			//配列の初期化
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				count[i] = 0;
			}
			//数量計算
			for (int i = 0; i < ordercode.Length; i++)
			{
				if (ordercode.Substring(i, 1) != ",")
				{
					count[int.Parse(ordercode.Substring(i, 1)) - 1] += 1;
				}
			}

			//合計金額を初期化
			total = 0;

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				dtRow = goods.NewRow();
				dtRow["商品ID"] = dt.Rows[i][0];      //DBから取得したdtの商品IDを示す行の行番号と列番号
				dtRow["商品名"] = dt.Rows[i][1];  //DBから取得したdtの商品名を示す行の行番号と列番号
				dtRow["単価"] = dt.Rows[i][2];        //DBから取得したdtの単価を示す行の行番号と列番号   
				price = dt.Rows[i][2].ToString();
				sum = int.Parse(price);
				dtRow["数量"] = count[i];
				total += sum * count[i];
				dtRow["売上"] = sum * count[i];
				goods.Rows.Add(dtRow);
			}

			Console.WriteLine(count[0]);

			//dataGridView1.DataSource = sal;
			//dataGridView1.DataMember = "goods";

			chart1.Series.Clear();
			chart1.Titles.Clear();
			chart1.Titles.Add("商品別の売上金額");
			chart1.Series.Add("売上");


			chart1.Series["売上"].ChartType = SeriesChartType.Bar;        //Bar
			chart1.Series["売上"].IsValueShownAsLabel = true;
			chart1.ChartAreas["ChartArea1"].AxisY.Interval = 500;

			chart1.DataSource = goods;         //チャートに表示するデータテーブルを設定
			chart1.Series["売上"].XValueMember = goods.Columns["商品名"].ColumnName;
			chart1.Series["売上"].YValueMembers = goods.Columns["売上"].ColumnName;
			chart1.DataBind();   //データバインド
			totalMsg.Text = "販売合計金額："+ total.ToString();
		}
	}
}
