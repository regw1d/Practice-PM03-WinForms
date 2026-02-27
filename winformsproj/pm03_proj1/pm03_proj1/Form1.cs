using System;
using System.Data;
using Microsoft.Data.Sqlite; // <-- ÍÎÂÀß ÁÈÁËÈÎÒÅÊÀ
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace pm03_proj1
{
    public partial class Form1 : Form
    {
        private string dbFileName = "calc_history.sqlite";
        private string connectionString;

        public Form1()
        {
            InitializeComponent();
            connectionString = $"Data Source={dbFileName}"; // Óïðîñòèëè ñòðîêó ïîäêëþ÷åíèÿ
            InitializeDatabase();
            LoadHistory();
            SetupChart();
        }

        private void InitializeDatabase()
        {
            // Èñïîëüçóåì íîâûå êëàññû: SqliteConnection, SqliteCommand
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Calculations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Principal REAL, 
                        Rate REAL, 
                        Years INTEGER, 
                        FinalAmount REAL, 
                        CalcDate TEXT
                    )";
                using (SqliteCommand cmd = new SqliteCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadHistory()
        {
            // Èñïîëüçóåì íîâûå êëàññû: SqliteConnection, SqliteDataAdapter
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string selectQuery = "SELECT Id as '¹', Principal as 'Ñóììà', Rate as 'Ñòàâêà(%)', Years as 'Ëåò', FinalAmount as 'Èòîã', CalcDate as 'Äàòà' FROM Calculations ORDER BY Id DESC";

                // SqliteDataAdapter íå ñóùåñòâóåò, äåëàåì âðó÷íóþ - ýòî íàäåæíåå
                DataTable dt = new DataTable();
                using (SqliteCommand cmd = new SqliteCommand(selectQuery, conn))
                {
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
                dataGridView1.DataSource = dt;
            }
        }

        private void SetupChart()
        {
            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.Titles.Add("Âû÷èñëèòåëüíûé ýêñïåðèìåíò: Ðîñò êàïèòàëà");
            chart1.ChartAreas[0].AxisX.Title = "Ãîä";
            chart1.ChartAreas[0].AxisY.Title = "Ñóììà";
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                double principal = Convert.ToDouble(txtPrincipal.Text.Replace('.', ','));
                double rate = Convert.ToDouble(txtRate.Text.Replace('.', ',')) / 100;
                int years = Convert.ToInt32(txtYears.Text);

                double currentAmount = principal;

                // --- ÍÀ×ÀËÎ: ÂÛ×ÈÑËÈÒÅËÜÍÛÉ ÝÊÑÏÅÐÈÌÅÍÒ È ÃÐÀÔÈÊ ---
                chart1.Series.Clear();
                Series series = chart1.Series.Add("Êàïèòàë");
                series.ChartType = SeriesChartType.Column;
                series.Color = System.Drawing.Color.MediumSeaGreen;
                series.IsValueShownAsLabel = true;

                for (int i = 1; i <= years; i++)
                {
                    currentAmount += currentAmount * rate;
                    series.Points.AddXY(i, Math.Round(currentAmount, 2));
                }
                // --- ÊÎÍÅÖ: ÂÛ×ÈÑËÈÒÅËÜÍÛÉ ÝÊÑÏÅÐÈÌÅÍÒ ---

                // Èñïîëüçóåì íîâûå êëàññû
                using (SqliteConnection conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    string insertQuery = "INSERT INTO Calculations (Principal, Rate, Years, FinalAmount, CalcDate) VALUES (@p, @r, @y, @f, @d)";
                    using (SqliteCommand cmd = new SqliteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@p", principal);
                        cmd.Parameters.AddWithValue("@r", rate * 100);
                        cmd.Parameters.AddWithValue("@y", years);
                        cmd.Parameters.AddWithValue("@f", Math.Round(currentAmount, 2));
                        cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Îøèáêà ââîäà äàííûõ! Óáåäèòåñü, ÷òî âû ââåëè ÷èñëà.\nÏîäðîáíîñòè: " + ex.Message, "Îøèáêà", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
