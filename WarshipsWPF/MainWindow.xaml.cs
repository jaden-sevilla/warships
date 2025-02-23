using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarshipsWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : Window
	{
		private char[,] Board = new char[10, 10];
		private ShipType[] Ships = new ShipType[5];
		private int hits;
		private int totalShots;
		private string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

		public struct ShipType
		{
			public string Name;
			public int Size;
		}

		public MainWindow()
		{
			InitializeComponent();
			MainMenu.Visibility = Visibility.Visible;
			BoardPanel.Visibility = Visibility.Hidden;
			Width = 1280;
			Height = 720;
		}

		private void Play_Click(object sender, RoutedEventArgs e)
		{
			MainMenu.Visibility = Visibility.Hidden;
			BoardPanel.Visibility = Visibility.Visible;
			WinGrid.Visibility = Visibility.Hidden;

			BoardGrid.RowDefinitions.Clear();
			BoardGrid.ColumnDefinitions.Clear();
			BoardGrid.Children.Clear();


			SetUpShips(ref Ships);
			SetUpBoard(ref Board);
			PlaceRandomShips(ref Board, Ships);

			Accuracy.Text = "-";
			NumberHit.Text = "0/0";
		}
		private void Board_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			PlayerMove(button);
		}

		private void PlayerMove(Button button)
		{
			int Row = Grid.GetRow(button);
			int Column = Grid.GetColumn(button);
			if (Board[Row, Column] == 'm' || Board[Row, Column] == 'h')
			{
				return;
			}
			else if (Board[Row, Column] == '-')
			{
				button.Background = Brushes.Transparent;
				Board[Row, Column] = 'm';
				totalShots++;

				string soundPath = System.IO.Path.Combine(projectPath, "Sounds", "splash.wav");
				System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath);
				player.Play();
			}
			else
			{
				Board[Row, Column] = 'h';
				button.Background = Brushes.Green;
				totalShots++;
				hits++;

				string soundPath = System.IO.Path.Combine(projectPath, "Sounds", "explosion.wav");
				System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath);
				player.Play();


			}
			button.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

			double percentage = (double)hits / (double)totalShots * 100;


			NumberHit.Text = $"{hits}/{totalShots}";
			Accuracy.Text = Math.Round(percentage) + "%";
			if (CheckWin(Board) == true)
			{
				WinGrid.Visibility = Visibility.Visible;
			}
		}

		public void SetUpBoard(ref char[,] Board)
		{
			hits = 0;
			totalShots = 0;
			for (int i = 0; i < 10; i++)
			{
				BoardGrid.RowDefinitions.Add(new RowDefinition());
			}
			for (int j = 0; j < 10; j++)
			{
				BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			for (int Row = 0; Row < 10; Row++)
			{
				for (int Column = 0; Column < 10; Column++)
				{
					Board[Row, Column] = '-';
					Button btn = new Button()
					{
						Content = "",
						Background = Brushes.LightGray,
					};
					btn.Style = (Style)this.Resources["NoHoverButton"];

					btn.Click += Board_Click;
					Grid.SetRow(btn, Row);
					Grid.SetColumn(btn, Column);
					BoardGrid.Children.Add(btn);
				}
			}
		}

		private static void SetUpShips(ref ShipType[] Ships)
		{
			Ships[0].Name = "Aircraft Carrier";
			Ships[0].Size = 5;
			Ships[1].Name = "Battleship";
			Ships[1].Size = 4;
			Ships[2].Name = "Submarine";
			Ships[2].Size = 3;
			Ships[3].Name = "Destroyer";
			Ships[3].Size = 3;
			Ships[4].Name = "Patrol Boat";
			Ships[4].Size = 2;
		}
		private static void PlaceRandomShips(ref char[,] Board, ShipType[] Ships)
		{
			Random RandomNumber = new Random();
			bool Valid;
			char Orientation = ' ';
			int Row = 0;
			int Column = 0;
			int HorV = 0;
			foreach (var Ship in Ships)
			{
				Valid = false;
				while (Valid == false)
				{
					Row = RandomNumber.Next(0, 10);
					Column = RandomNumber.Next(0, 10);
					HorV = RandomNumber.Next(0, 2);
					if (HorV == 0)
					{
						Orientation = 'v';
					}
					else
					{
						Orientation = 'h';
					}
					Valid = ValidateBoatPosition(Board, Ship, Row, Column, Orientation);
				}
				Console.WriteLine("Computer placing the " + Ship.Name);
				PlaceShip(ref Board, Ship, Row, Column, Orientation);
			}
		}

		private static void PlaceShip(ref char[,] Board, ShipType Ship, int Row, int Column, char Orientation)
		{
			if (Orientation == 'v')
			{
				for (int Scan = 0; Scan < Ship.Size; Scan++)
				{
					Board[Row + Scan, Column] = Ship.Name[0];
				}
			}
			else if (Orientation == 'h')
			{
				for (int Scan = 0; Scan < Ship.Size; Scan++)
				{
					Board[Row, Column + Scan] = Ship.Name[0];
				}
			}
		}
		private static bool ValidateBoatPosition(char[,] Board, ShipType Ship, int Row, int Column, char Orientation)
		{
			if (Orientation == 'v' && Row + Ship.Size > 10)
			{
				return false;
			}
			else if (Orientation == 'h' && Column + Ship.Size > 10)
			{
				return false;
			}
			else
			{
				if (Orientation == 'v')
				{
					for (int Scan = 0; Scan < Ship.Size; Scan++)
					{
						if (Board[Row + Scan, Column] != '-')
						{
							return false;
						}
					}
				}
				else if (Orientation == 'h')
				{
					for (int Scan = 0; Scan < Ship.Size; Scan++)
					{
						if (Board[Row, Column + Scan] != '-')
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool CheckWin(char[,] Board)
		{
			for (int Row = 0; Row < 10; Row++)
			{
				for (int Column = 0; Column < 10; Column++)
				{
					if (Board[Row, Column] == 'A' || Board[Row, Column] == 'B' || Board[Row, Column] == 'S' || Board[Row, Column] == 'D' || Board[Row, Column] == 'P')
					{
						return false;
					}
				}
			}
			foreach (Button btn in BoardGrid.Children)
			{
				btn.IsEnabled = false;
			}

			string soundPath = System.IO.Path.Combine(projectPath, "Sounds", "win.wav");
			System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundPath);
			player.Play();

			return true;
		}

		private void MainMenu_Click(object sender, RoutedEventArgs e)
		{
			MainMenu.Visibility = Visibility.Visible;
			BoardPanel.Visibility = Visibility.Hidden;
		}

		private void Quit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}