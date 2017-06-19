using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace CheckersBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Move currentMove;
        private String winner;
        private String turn;

        private CheckerBoard board;
        private Player currentPlayer = Player.Black;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Checkers! Blacks turn!";
            currentMove = null;
            winner = null;
            turn = "Black";

            // Dependencies
            this.randomService = new RandomService();
            this.listHelper = new ListHelpers(this.randomService);

            var mctsRandom = new RandomService();

            // Define algorithms
            this.algorithms = new Dictionary<Player, IAIAlgorithm>
            {
                {Player.Black, new MctsAI(new ListHelpers(mctsRandom), mctsRandom)},
                {Player.Red, new DfsAI(new ListHelpers(new RandomService()))}
            };

            board = new CheckerBoard();
            board.InitializeBoard();

            MakeBoard();
            DrawStates(board);
        }

        private void ClearBoard()
        {
            for (int r = 1; r < 9; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r, c);
                    CheckersGrid.Children.Remove(stackPanel);
                }
            }
        }

        private void MakeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = new StackPanel();
                    if (r % 2 != c % 2)
                        stackPanel.Background = new SolidColorBrush(Color.FromRgb(162, 95, 24));
                    else
                        stackPanel.Background = new SolidColorBrush(Color.FromRgb(219, 166, 90));

                    Grid.SetRow(stackPanel, r+1);
                    Grid.SetColumn(stackPanel, c);
                    CheckersGrid.Children.Add(stackPanel);
                }
            }

            //MakeButtons();
        }

        private void DrawStates(CheckerBoard board)
        {
            var images = new Dictionary<FieldState, ImageBrush>
            {
                {FieldState.Red, new ImageBrush(new BitmapImage(new Uri("Resources/red60p.png", UriKind.Relative))) },
                {FieldState.RedKing, new ImageBrush(new BitmapImage(new Uri("Resources/red60p_king.png", UriKind.Relative))) },
                {FieldState.Black, new ImageBrush(new BitmapImage(new Uri("Resources/black60p.png", UriKind.Relative))) },
                {FieldState.BlackKing, new ImageBrush(new BitmapImage(new Uri("Resources/black60p_king.png", UriKind.Relative))) }
            };

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var state = board.GetState(r, c);
                    StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r + 1, c);

                    if (state != FieldState.Empty && state != FieldState.Invalid)
                    {
                        Button button = new Button();
                        button.Click += new RoutedEventHandler(button_Click);
                        button.Height = 60;
                        button.Width = 60;
                        button.HorizontalAlignment = HorizontalAlignment.Center;
                        button.VerticalAlignment = VerticalAlignment.Center;
                        button.Background = images[state];
                        stackPanel.Children.Add(button);
                    }
                    else
                    {
                        if (stackPanel.Children.Count > 0)
                            stackPanel.Children.RemoveRange(0, stackPanel.Children.Count);
                    }
                }
            }
        }

        private IRandomService randomService;
        private IListHelper listHelper;

        private Dictionary<Player, IAIAlgorithm> algorithms;

        private async void playAutomated_Click(object sender, RoutedEventArgs e)
        {
            int i = 1;
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                while (board.GetGameStatus() == GameStatuses.Running)
                {
                    updateStatusBar(i);

                    stopwatch.Reset();
                    stopwatch.Start();
                    var move = algorithms[currentPlayer].GetMove(board, currentPlayer);
                    stopwatch.Stop();

                    if (move == null)
                    {
                        MessageBox.Show("No more moves");
                        break;
                    }

                    Console.WriteLine($"Turn #{i++}; Player {currentPlayer}; ({move.piece1.Row}, {move.piece1.Column}) to ({move.piece2.Row}, {move.piece2.Column}); Elapsed: {stopwatch.ElapsedMilliseconds} ms");

                    board.MakeMove(move, currentPlayer);
                    DrawStates(board);

                    if (board.GetGameStatus() != GameStatuses.Running)
                        continue;

                    if (currentPlayer == Player.Red)
                        currentPlayer = Player.Black;
                    else
                        currentPlayer = Player.Red;

                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EXCEPTION");
            }

            if (board.GetGameStatus() != GameStatuses.Running)
                MessageBox.Show($"Done! Player {currentPlayer} won!");
        }

        private void updateStatusBar(int turn)
        {
            this.nextPlayerField.Text = board.NextPlayer.ToString();
            this.turnNumField.Text = turn.ToString();
        }

        private int manualTurn = 1;
        private void nextMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var move = algorithms[currentPlayer].GetMove(board, currentPlayer);
                if (move == null)
                {
                    MessageBox.Show("No more moves");
                    return;
                }

                Debug.WriteLine($"Player {currentPlayer}; ({move.piece1.Row}, {move.piece1.Column}) to ({move.piece2.Row}, {move.piece2.Column})");


                board.MakeMove(move, currentPlayer);
                DrawStates(board);

                /*if (currentPlayer == Player.Red)
                    currentPlayer = Player.Black;
                else
                    currentPlayer = Player.Red;*/

                currentPlayer = board.NextPlayer;

                updateStatusBar(++manualTurn);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EXCEPTION");
            }
        }

        private void MakeButtons()
        {
            for (int r = 1; r < 9; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r, c);
                    Button button = new Button();
                    button.Click += new RoutedEventHandler(button_Click);
                    button.Height = 60;
                    button.Width = 60;
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.VerticalAlignment = VerticalAlignment.Center;
                    var redBrush = new ImageBrush();
                    redBrush.ImageSource = new BitmapImage(new Uri("Resources/red60p.png", UriKind.Relative));
                    var blackBrush = new ImageBrush();
                    blackBrush.ImageSource = new BitmapImage(new Uri("Resources/black60p.png", UriKind.Relative));
                    switch (r)
                    {
                        case 1:
                            if (c % 2 == 1)
                            {

                                button.Background = redBrush;
                                button.Name = "buttonRed" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 2:
                            if (c % 2 == 0)
                            {
                                button.Background = redBrush;
                                button.Name = "buttonRed" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 3:
                            if (c % 2 == 1)
                            {
                                button.Background = redBrush;
                                button.Name = "buttonRed" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 4:
                            if (c % 2 == 0)
                            {
                                button.Background = Brushes.Black;
                                button.Name = "button" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 5:
                            if (c % 2 == 1)
                            {
                                button.Background = Brushes.Black;
                                button.Name = "button" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 6:
                            if (c % 2 == 0)
                            {
                                button.Background = blackBrush;
                                button.Name = "buttonBlack" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 7:
                            if (c % 2 == 1)
                            {
                                button.Background = blackBrush;
                                button.Name = "buttonBlack" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        case 8:
                            if (c % 2 == 0)
                            {
                                button.Background = blackBrush;
                                button.Name = "buttonBlack" + r + c;
                                stackPanel.Children.Add(button);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        UIElement GetGridElement(Grid g, int r, int c)
        {
            for (int i = 0; i < g.Children.Count; i++)
            {
                UIElement e = g.Children[i];
                if (Grid.GetRow(e) == r && Grid.GetColumn(e) == c)
                    return e;
            }
            return null;
        }

        public void button_Click(Object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            int row = Grid.GetRow(stackPanel);
            int col = Grid.GetColumn(stackPanel);
            Console.WriteLine("Row: " + row + " Column: " + col);
            if (currentMove == null)
                currentMove = new Move();
            if (currentMove.piece1 == null)
            {
                currentMove.piece1 = new Piece(row, col);
                stackPanel.Background = Brushes.Green;
            }
            else
            {
                currentMove.piece2 = new Piece(row, col);
                stackPanel.Background = Brushes.Green;
            }
            if ((currentMove.piece1 != null) && (currentMove.piece2 != null))
            {
                if (CheckMove())
                {
                    MakeMove();
                    aiMakeMove();
                }
            }
        }

        private Boolean CheckMove()
        {
            StackPanel stackPanel1 = (StackPanel)GetGridElement(CheckersGrid, currentMove.piece1.Row, currentMove.piece1.Column);
            StackPanel stackPanel2 = (StackPanel)GetGridElement(CheckersGrid, currentMove.piece2.Row, currentMove.piece2.Column);
            Button button1 = (Button) stackPanel1.Children[0];
            Button button2 = (Button) stackPanel2.Children[0];
            stackPanel1.Background = Brushes.Black;
            stackPanel2.Background = Brushes.Black;

            if ((turn == "Black") && (button1.Name.Contains("Red")))
            {
                currentMove.piece1 = null;
                currentMove.piece2 = null;
                displayError("It is blacks turn.");
                return false;
            }
            if ((turn == "Red") && (button1.Name.Contains("Black")))
            {
                currentMove.piece1 = null;
                currentMove.piece2 = null;
                displayError("It is reds turn.");
                return false;
            }
            if (button1.Equals(button2))
            {
                currentMove.piece1 = null;
                currentMove.piece2 = null;
                return false;
            }
            if(button1.Name.Contains("Black"))
            {
                return CheckMoveBlack(button1, button2);
            }
            else if (button1.Name.Contains("Red"))
            {
                return CheckMoveRed(button1, button2);
            }
            else
            {
                currentMove.piece1 = null;
                currentMove.piece2 = null;
                Console.WriteLine("False");
                return false;
            }
        }

        private bool CheckMoveRed(Button button1, Button button2)
        {
            CheckerBoard currentBoard = GetCurrentBoard();
            var jumpMoves = currentBoard.GetJumpMoves(Player.Red);

            if (jumpMoves.Count > 0)
            {
                bool invalid = true;
                foreach (Move move in jumpMoves)
                {
                    if (currentMove.Equals(move))
                        invalid = false;
                }
                if (invalid)
                {
                    displayError("Jump must be taken");
                    currentMove.piece1 = null;
                    currentMove.piece2 = null;
                    Console.WriteLine("False");
                    return false;
                }
            }

            if (button1.Name.Contains("Red"))
            {
                if (button1.Name.Contains("King"))
                {
                    if ((currentMove.isAdjacent("King")) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                        return true;
                    Piece middlePiece = currentMove.checkJump("King");
                    if ((middlePiece != null) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                    {
                        StackPanel middleStackPanel = (StackPanel)GetGridElement(CheckersGrid, middlePiece.Row, middlePiece.Column);
                        Button middleButton = (Button)middleStackPanel.Children[0];
                        if (middleButton.Name.Contains("Black"))
                        {
                            CheckersGrid.Children.Remove(middleStackPanel);
                            addBlackButton(middlePiece);
                            return true;
                        }
                    }
                }
                else
                {
                    if ((currentMove.isAdjacent("Red")) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                        return true;
                    Piece middlePiece = currentMove.checkJump("Red");
                    if ((middlePiece != null) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                    {
                        StackPanel middleStackPanel = (StackPanel)GetGridElement(CheckersGrid, middlePiece.Row, middlePiece.Column);
                        Button middleButton = (Button)middleStackPanel.Children[0];
                        if (middleButton.Name.Contains("Black"))
                        {
                            CheckersGrid.Children.Remove(middleStackPanel);
                            addBlackButton(middlePiece);
                            return true;
                        }
                    }
                }
            }
            currentMove = null;
            displayError("Invalid Move. Try Again.");
            return false;
        }

        private bool CheckMoveBlack(Button button1, Button button2)
        {
            CheckerBoard currentBoard = GetCurrentBoard();
            var jumpMoves = currentBoard.GetJumpMoves(Player.Black);

            if (jumpMoves.Count > 0)
            {
                bool invalid = true;
                foreach (Move move in jumpMoves)
                {
                    if (currentMove.Equals(move))
                        invalid = false;
                }
                if (invalid)
                {
                    displayError("Jump must be taken");
                    currentMove.piece1 = null;
                    currentMove.piece2 = null;
                    Console.WriteLine("False");
                    return false;
                }
            }

            if (button1.Name.Contains("Black"))
            {
                if (button1.Name.Contains("King"))
                {
                    if ((currentMove.isAdjacent("King")) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                        return true;
                    Piece middlePiece = currentMove.checkJump("King");
                    if ((middlePiece != null) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                    {
                        StackPanel middleStackPanel = (StackPanel)GetGridElement(CheckersGrid, middlePiece.Row, middlePiece.Column);
                        Button middleButton = (Button)middleStackPanel.Children[0];
                        if (middleButton.Name.Contains("Red"))
                        {
                            CheckersGrid.Children.Remove(middleStackPanel);
                            addBlackButton(middlePiece);
                            return true;
                        }
                    }
                }
                else
                {
                    if ((currentMove.isAdjacent("Black")) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                        return true;
                    Piece middlePiece = currentMove.checkJump("Black");
                    if ((middlePiece != null) && (!button2.Name.Contains("Black")) && (!button2.Name.Contains("Red")))
                    {
                        StackPanel middleStackPanel = (StackPanel)GetGridElement(CheckersGrid, middlePiece.Row, middlePiece.Column);
                        Button middleButton = (Button)middleStackPanel.Children[0];
                        if (middleButton.Name.Contains("Red"))
                        {
                            CheckersGrid.Children.Remove(middleStackPanel);
                            addBlackButton(middlePiece);
                            return true;
                        }
                    }
                }
            }
            currentMove = null;
            displayError("Invalid Move. Try Again.");
            return false;
       }

        private void MakeMove()
        {
            if ((currentMove.piece1 != null) && (currentMove.piece2 != null))
            {
                Console.WriteLine("Piece1 " + currentMove.piece1.Row + ", " + currentMove.piece1.Column);
                Console.WriteLine("Piece2 " + currentMove.piece2.Row + ", " + currentMove.piece2.Column);
                StackPanel stackPanel1 = (StackPanel)GetGridElement(CheckersGrid, currentMove.piece1.Row, currentMove.piece1.Column);
                StackPanel stackPanel2 = (StackPanel)GetGridElement(CheckersGrid, currentMove.piece2.Row, currentMove.piece2.Column);
                CheckersGrid.Children.Remove(stackPanel1);
                CheckersGrid.Children.Remove(stackPanel2);
                Grid.SetRow(stackPanel1, currentMove.piece2.Row);
                Grid.SetColumn(stackPanel1, currentMove.piece2.Column);
                CheckersGrid.Children.Add(stackPanel1);
                Grid.SetRow(stackPanel2, currentMove.piece1.Row);
                Grid.SetColumn(stackPanel2, currentMove.piece1.Column);
                CheckersGrid.Children.Add(stackPanel2);
                checkKing(currentMove.piece2);
                currentMove = null;
                if (turn == "Black")
                {
                    this.Title = "Checkers! Reds turn!";
                    turn = "Red";
                }
                else if (turn == "Red")
                {
                    this.Title = "Checkers! Blacks turn!";
                    turn = "Black";
                }
                checkWin();
            }
        }

        private void aiMakeMove()
        {
            currentMove = (new AgresiveRandomAI()).GetMove(GetCurrentBoard(), Player.Red);
            if (currentMove != null)
            {
                if (CheckMove())
                {
                    MakeMove();
                }
            }
        }

        private CheckerBoard GetCurrentBoard()
        {
            CheckerBoard board = new CheckerBoard();
            for (int r = 1; r < 9; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r, c);
                    if (stackPanel.Children.Count > 0)
                    {
                        Button button = (Button)stackPanel.Children[0];
                        if (button.Name.Contains("Red"))
                        {
                            if (button.Name.Contains("King"))
                                board.SetState(r - 1, c, FieldState.RedKing);
                            else
                                board.SetState(r - 1, c, FieldState.Red);
                        }
                        else if (button.Name.Contains("Black"))
                        {
                            if (button.Name.Contains("King"))
                                board.SetState(r - 1, c, FieldState.BlackKing);
                            else
                                board.SetState(r - 1, c, FieldState.Black);
                        }
                        else
                            board.SetState(r - 1, c, 0);

                    }
                    else
                    {
                        board.SetState(r - 1, c, FieldState.Invalid);
                    }

                }
            }
            return board;
        }

        private void checkKing(Piece tmpPiece)
        {
            StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, tmpPiece.Row, tmpPiece.Column);
            if (stackPanel.Children.Count > 0)
            {
                Button button = (Button)stackPanel.Children[0];
                var redBrush = new ImageBrush();
                redBrush.ImageSource = new BitmapImage(new Uri("Resources/red60p_king.png", UriKind.Relative));
                var blackBrush = new ImageBrush();
                blackBrush.ImageSource = new BitmapImage(new Uri("Resources/black60p_king.png", UriKind.Relative));
                if ((button.Name.Contains("Black")) && (!button.Name.Contains("King")))
                {
                    if (tmpPiece.Row == 1)
                    {
                        button.Name = "button" + "Black" + "King" + tmpPiece.Row + tmpPiece.Column;
                        button.Background = blackBrush;
                    }
                }
                else if ((button.Name.Contains("Red")) && (!button.Name.Contains("King")))
                {
                    if (tmpPiece.Row == 8)
                    {
                        button.Name = "button" + "Red" + "King" + tmpPiece.Row + tmpPiece.Column;
                        button.Background = redBrush;
                    }
                }
            }
        }
        
        private void addBlackButton(Piece middleMove)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Background = Brushes.Black;
            Button button = new Button();
            button.Click += new RoutedEventHandler(button_Click);
            button.Height = 60;
            button.Width = 60;
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.VerticalAlignment = VerticalAlignment.Center;
            button.Background = Brushes.Black;
            button.Name = "button" + middleMove.Row + middleMove.Column;
            stackPanel.Children.Add(button);
            Grid.SetColumn(stackPanel, middleMove.Column);
            Grid.SetRow(stackPanel, middleMove.Row);
            CheckersGrid.Children.Add(stackPanel);
        }

        private void checkWin()
        {
            int totalBlack = 0, totalRed = 0;
            for (int r = 1; r < 9; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r, c);
                    if (stackPanel.Children.Count > 0)
                    {
                        Button button = (Button)stackPanel.Children[0];
                        if (button.Name.Contains("Red"))
                            totalRed++;
                        if (button.Name.Contains("Black"))
                            totalBlack++;
                    }
                }
            }
            if (totalBlack == 0)
                winner = "Red";
            if (totalRed == 0)
                winner = "Black";
            if (winner != null)
            {
                for (int r = 1; r < 9; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        StackPanel stackPanel = (StackPanel)GetGridElement(CheckersGrid, r, c);
                        if (stackPanel.Children.Count > 0)
                        {
                            Button button = (Button)stackPanel.Children[0];
                            button.Click -= new RoutedEventHandler(button_Click);
                        }
                    }
                }
                MessageBoxResult result = MessageBox.Show(winner + " is the winner! Would you like to play another?", "Winner", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    newGame();
            }
        }

        private void newGame()
        {
            currentMove = null;
            winner = null;
            this.Title = "Checkers! Blacks turn!";
            turn = "Black";
            ClearBoard();
            MakeBoard();
        }

        private void displayError(string error)
        {
            MessageBox.Show(error, "Invalid Move", MessageBoxButton.OK);
        }

        private void newGame_Click(object sender, RoutedEventArgs e)
        {
            newGame();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                nextMove_Click(sender, null);
            }
        }
    }
}
