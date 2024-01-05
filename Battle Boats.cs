using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Battleships
{
    //remove save abuse after having shot
    //add overall stats
    //multiplayer and leaderboard
    //controls at bottom of main screen after time

    internal class Program {
        static string[] title = ["|---\\ /---\\ --|-- --|-- |     /----       |---\\ /---\\ /---\\ --|-- /---\n",
                                 " |---| |---|   |     |   |     |----       |---| |   | |---|   |   \\---\\\n",
                                 " |---/ |   |   |     |   \\---- \\----       |---/ \\---/ |   |   |    ---/\n"]; //Title ASCII art

        static Option[] menuOptions = new Option[] { //Set out options for main menu, string connected with functions
            new Option(NewGame, "New Game"),
            new Option(Resume, "Resume"),
            new Option(Instructions, "Instructions"),
            new Option(Quit, "Exit")
        };

        static string[] instructions = {
            "Controls: ",
            "Move up/down menus - W or Up arrow, S or Down arrow",
            "Increase/Decrease menu option - D or Right Arrow, A or Left Arrow",
            "Toggle menu option - A, D, Left or Right Arrow",
            "Select menu potion - Enter or C",
            "Enter text field - Enter or C to start, Enter at end",
            "Delete file in file select - Delete or Backspace",
            "",
            "Rotate ship during placement - Q or E",
            "Move ship during placement - WASD or Arrow keys",
            "Next ship - Tab",
            "",
            "Move target - WASD or Arrow keys",
            "Confirm target - Enter or C",
            "Acknowledge message - any key",
            "Exit game - Escape (Progress is automatically saved",
            "",
            new string ('-', 50),
            "",
            "How to play: ",
            "Select game options and save name",
            "Select positions of your battleships",
            "Confirm targetting on one of your opponents squares",
            "You are told if this hits, and additionally if it sink their ship",
            "The opponent will then take their turn, and you will recieve information about their shot",
            "First player to destroy all of their opponents ships wins",
            "",
            "Good Luck!"
        }; //Instructions that can be displayed via menu

        static int menuLength;
        static int boardGap = 20;

        static Random random = new Random();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        static PlayerBoard playerBoard;
        static ComputerBoard computerBoard;
        static bool isPlayerTurn; //static to help loading save files

        static string assemblyPath; //strings to help saving/loading files
        static string savePath;
        static string currentSaveLocation;
        static string currentSaveName;

        static bool updatedDisplay = false;

        static void Main(string[] args)
        {
            UpdateName("Unamed Save"); //sets default save name
            Console.ForegroundColor = ConsoleColor.DarkGreen; //set text colour
            menuLength = menuOptions.Length;

            if (!Directory.Exists(savePath)) //Creates save folder if it doesn't exist
            {
                Directory.CreateDirectory(savePath);
            }

            Menu(); //Starts game by calling menu
        }
        static void UpdateName(string saveName) //Updates all strings used for saving files
        {
            currentSaveName = saveName;
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            savePath = assemblyPath + "/Save Files";
            currentSaveLocation = savePath + "/" + currentSaveName;
        }
        static void NewGame()
        {
            int playerWidth = 8; //Sets default Values
            int playerHeight = 8;
            int computerHeight = 8;
            int computerWidth = 8;
            int[] playerShipNums = new int[5] { 0, 1, 2, 1, 1 };
            int[] computerShipNums = new int[5] { 0, 1, 2, 1, 1 };
            isPlayerTurn = true;

            bool finishedSetup = false;
            int currentSelected = 0;

            while (!finishedSetup)
            {
                string[] options = { //Displays current options
                    $"Player board width: {playerWidth}",
                    $"Player board height: {playerHeight}",
                    $"Computer board width: {computerWidth}",
                    $"Computer board height: {computerHeight}",
                    $"Player starts: {isPlayerTurn}",
                    "", //5
                    $"Player ships - ones: {playerShipNums[0]}",
                    $"Player ships - twos: {playerShipNums[1]}",
                    $"Player ships - threes: {playerShipNums[2]}",
                    $"Player ships - fours: {playerShipNums[3]}",
                    $"Player ships - fives: {playerShipNums[4]}", //10
                    "",
                    $"Computer ships - ones: {computerShipNums[0]}",
                    $"Computer ships - twos: {computerShipNums[1]}",
                    $"Computer ships - threes: {computerShipNums[2]}",
                    $"Computer ships - fours: {computerShipNums[3]}", //15
                    $"Computer ships - fives: {computerShipNums[4]}",
                    "",
                    "Start",
                    "Quit",
                    $"Save Name: {currentSaveName}",  //20
                    };
                string[] currentOptions = new string[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    if (currentSelected == i)
                    {
                        currentOptions[i] = " - " + options[i];
                    } else
                    {
                        currentOptions[i] = "   " + options[i];
                    }
                }
                string text = BasicText(currentOptions, true);
                Console.Clear();
                Console.Write(text);
                switch (Console.ReadKey().Key) //Accepts user input
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        currentSelected--;
                        currentSelected = (currentSelected + options.Length) % options.Length;
                        while (options[currentSelected] == "")
                        {
                            currentSelected--;
                            currentSelected = (currentSelected + options.Length) % options.Length;
                        }
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        currentSelected++;
                        currentSelected = currentSelected % options.Length;
                        while (options[currentSelected] == "")
                        {
                            currentSelected++;
                            currentSelected = currentSelected % options.Length;
                        }
                        break;
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.C:
                    case ConsoleKey.Enter:
                        if (currentSelected == 18)
                        {
                            finishedSetup = true;
                        } else if (currentSelected == 19)
                        {
                            return;
                        } else if (currentSelected == 20)
                        {
                            List<string> temporaryDisplay = currentOptions.ToList();
                            temporaryDisplay.Add("   Name: ");
                            text = BasicText(temporaryDisplay.ToArray(), true);
                            Console.Clear();
                            Console.Write(text);
                            string newSaveName = Console.ReadLine();
                            if (newSaveName.Length > 40)
                            {
                                temporaryDisplay = currentOptions.ToList();
                                temporaryDisplay.Add($"   Name: {newSaveName.Substring(0, 40)}...");
                                temporaryDisplay.Add("New name is too long; must be under 40 characters!");
                                text = BasicText(temporaryDisplay.ToArray(), true);
                                Console.Clear();
                                Console.Write(text);
                                Console.ReadKey();
                            } else
                            {
                                UpdateName(newSaveName);
                            }
                        }
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        switch (currentSelected)
                        {
                            case 0:
                                playerWidth++;
                                break;
                            case 1:
                                playerHeight++;
                                break;
                            case 2:
                                computerWidth++;
                                break;
                            case 3:
                                computerHeight++;
                                break;
                            case 4:
                                isPlayerTurn = !isPlayerTurn;
                                break;
                        }
                        if (currentSelected >= 6 && currentSelected <= 10)
                        {
                            playerShipNums[currentSelected - 6]++;
                        } else if (currentSelected >= 12 && currentSelected <= 16)
                        {
                            computerShipNums[currentSelected - 12]++;
                        }

                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        switch (currentSelected)
                        {
                            case 0:
                                playerWidth = int.Max(playerWidth - 1, 1);
                                break;
                            case 1:
                                playerHeight = int.Max(playerHeight - 1, 1);
                                break;
                            case 2:
                                computerWidth = int.Max(computerWidth - 1, 1);
                                break;
                            case 3:
                                computerHeight = int.Max(computerHeight - 1, 1);
                                break;
                            case 4:
                                isPlayerTurn = !isPlayerTurn;
                                break;
                        }
                        if (currentSelected >= 6 && currentSelected <= 10)
                        {
                            playerShipNums[currentSelected - 6] = int.Max(playerShipNums[currentSelected - 6] - 1, 0);
                        }
                        else if (currentSelected >= 12 && currentSelected <= 16)
                        {
                            computerShipNums[currentSelected - 12] = int.Max(computerShipNums[currentSelected - 12] - 1, 0);
                        }

                        break;

                }
            }

            List<int> shipsToPlace = new() {}; //Creates lists of unplaced ships for player and computer
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < playerShipNums[i]; j++)
                {
                    shipsToPlace.Add(i+1);
                }
            }

            List<int> computerShipsToPlace = new() { };
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < computerShipNums[i]; j++)
                {
                    computerShipsToPlace.Add(i + 1);
                }
            }

            List<Ship> ships = new();
            int currentX = 0;
            int currentY = 0;
            

            bool updateDisplay = true;
            while (shipsToPlace.Count > 0) {
                int currentShipIndex = 0;
                int currentShipLength = shipsToPlace[0];
                bool isVertical = false;
                bool foundLocation = false;
                while (!foundLocation)
                {
                    if (updateDisplay)
                    {
                        string text = PlacingDisplay(playerWidth, playerHeight, ships, currentX, currentY, currentShipLength, isVertical, shipsToPlace, currentShipIndex);
                        Console.Clear();
                        Console.WriteLine(text);
                        updateDisplay = false;
                    }
                    
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if ((isVertical && currentX != playerWidth - 1) || (!isVertical && currentX != playerWidth - currentShipLength)) {
                                currentX++;
                                updateDisplay = true;
                            }
                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            if (currentX != 0) {
                                currentX--;
                                updateDisplay = true;
                            }
                            break;
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            if (currentY != 0) {
                                currentY--;
                                updateDisplay = true;
                            }
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            if ((!isVertical && currentY != playerHeight - 1) || (isVertical && currentY != playerHeight - currentShipLength)) {
                                currentY = Math.Min(currentY + 1, playerHeight - 1);
                                updateDisplay = true;
                            }
                            break;
                        case ConsoleKey.Tab:
                            currentShipIndex= (currentShipIndex + 1) % shipsToPlace.Count;
                            updateDisplay = true;
                            currentShipLength = shipsToPlace[currentShipIndex];
                            if (isVertical) {
                                currentY = Math.Min(currentY, playerHeight - currentShipLength);
                            } else {
                                currentX = Math.Min(currentX, playerWidth - currentShipLength);
                            }
                            break;
                        case ConsoleKey.Q:
                        case ConsoleKey.E:
                            isVertical = !isVertical;
                            updateDisplay = true;
                            if (isVertical) {
                                currentY = Math.Min(currentY, playerHeight - currentShipLength);
                            } else {
                                currentX = Math.Min(currentX, playerWidth - currentShipLength);
                            }
                            break;
                        case ConsoleKey.C:
                        case ConsoleKey.Enter:
                            bool validLocation = true;
                            Ship shipToAdd = new Ship(currentShipLength, isVertical, new Vector2(currentX, currentY));
                            foreach (Ship s in ships) {
                                foreach (Vector2 otherSquare in s.GetSquares())
                                {
                                    foreach (Vector2 thisSquare in shipToAdd.GetSquares())
                                    {
                                        if (thisSquare.X == otherSquare.X && thisSquare.Y == otherSquare.Y)
                                        {
                                            validLocation = false;
                                        }
                                    }
                                }
                            }

                            if (validLocation) {
                                updateDisplay = true;
                                ships.Add(new Ship(currentShipLength, isVertical, new Vector2(currentX, currentY)));
                                shipsToPlace.RemoveAt(currentShipIndex);
;                               foundLocation = true;
                                if (shipsToPlace.Count != 0)
                                {
                                    currentShipLength = shipsToPlace[0];
                                    if (isVertical)
                                    {
                                        currentY = Math.Min(currentY, playerHeight - currentShipLength);
                                    }
                                    else
                                    {
                                        currentX = Math.Min(currentX, playerWidth - currentShipLength);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            List<Ship> computerShips = new();

            while (computerShipsToPlace.Count != 0)
            {
                bool validLocation = true;
                int selectedShipIndex = random.Next(computerShipsToPlace.Count);
                int selectedShipLength = computerShipsToPlace[selectedShipIndex];
                bool isVertical = random.Next(1) == 0;
                int x, y;
                if (isVertical)
                {
                    x = random.Next(computerWidth);
                    y = random.Next(computerHeight - selectedShipLength + 1);
                } else
                {
                    x = random.Next(computerWidth - selectedShipLength + 1);
                    y = random.Next(computerHeight);
                }
                Ship ship = new Ship(selectedShipLength, isVertical, new Vector2(x, y));

                foreach (Ship s in computerShips)
                {
                    foreach (Vector2 v in s.GetSquares())
                    {
                        foreach (Vector2 part in ship.GetSquares())
                        {
                            if (part == v)
                            {
                                validLocation = false;
                            }
                        }
                    }
                }

                if (validLocation)
                {
                    computerShipsToPlace.RemoveAt(selectedShipIndex);
                    computerShips.Add(ship);
                }
            }

            playerBoard = new PlayerBoard(ships.ToArray(), playerWidth, playerHeight);
            computerBoard = new ComputerBoard(computerShips.ToArray(), computerWidth, computerHeight);
            GameLoop();
        }
        static void Resume()
        {
            string text;
            bool newFileSet = false;
            
            bool fileSelected = false;

            string[] files = Directory.GetFiles(savePath);
            if (files.Length == 0)
            {
                text = BasicText(["You currently have no saved files", "(press any key to return to menu)"], true);
                Console.Clear();
                Console.WriteLine(text);
                Console.ReadKey();
                return;
            }
            int options = files.Length + 1;
            int selected = 0;
            int pathTrimLength = savePath.Length + 1;

            while (!fileSelected)
            {
                if (newFileSet)
                {
                    files = Directory.GetFiles(savePath);
                    if (files.Length == 0)
                    {
                        text = BasicText(["You currently have no saved files", "(press any key to return to menu)"], true);
                        Console.Clear();
                        Console.WriteLine(text);
                        Console.ReadKey();
                        return;
                    }
                    options = files.Length + 1;
                    selected = 0;
                    pathTrimLength = savePath.Length + 1;
                }

                List<string> lines = new();

                for (int i = 0; i < files.Length; i++)
                {
                    if (selected == i)
                    {
                        lines.Add(" - " + files[i].Substring(pathTrimLength) + new string(' ', 50 - files[i].Substring(pathTrimLength).Length) + File.GetLastAccessTime(files[i]));
                    } else
                    {
                        lines.Add("   " + files[i].Substring(pathTrimLength) + new string(' ', 50 - files[i].Substring(pathTrimLength).Length) + File.GetLastAccessTime(files[i]));
                    }
                }
                lines.Add((selected == options - 1) ? " - Quit" : "   Quit");

                text = BasicText(lines.ToArray(), true);
                Console.Clear();
                Console.WriteLine(text);
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.C:
                    case ConsoleKey.Enter:
                        fileSelected = true;
                        break;
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        if (selected != options - 1)
                        {
                            File.Delete(files[selected]);
                            newFileSet = true;
                        }
                        break;
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        selected = (selected - 1 + options) % options;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options;
                        break;
                }
            }

            if (selected == options - 1) {return;}

            string chosenFileName = files[selected];

            BinaryReader reader = new BinaryReader(File.Open(chosenFileName, FileMode.Open));

            isPlayerTurn = reader.ReadBoolean();
            int playerWidth = reader.ReadInt32();
            int playerHeight = reader.ReadInt32();
            int computerWidth = reader.ReadInt32();
            int computerHeight = reader.ReadInt32();
            int playerShipCount = reader.ReadInt32();
            int computerShipCount = reader.ReadInt32();

            Ship[] playerShips = new Ship[playerShipCount];
            Ship[] computerShips = new Ship[computerShipCount];
            bool[,] playerShots = new bool[playerWidth, playerHeight];
            bool[,] computerShots = new bool[computerWidth, computerHeight];

            for (int x = 0; x < playerWidth; x++)
            {
                for (int y = 0; y < playerHeight; y++)
                {
                    playerShots[x, y] = reader.ReadBoolean();
                }
            }

            for (int x = 0; x < computerWidth; x++)
            {
                for (int y = 0; y < computerHeight; y++)
                {
                    computerShots[x, y] = reader.ReadBoolean();
                }
            }

            for (int i = 0; i < playerShipCount; i++)
            {
                playerShips[i] = new Ship(reader.ReadInt32(), reader.ReadBoolean(), new Vector2(reader.ReadInt32(), reader.ReadInt32()));
            }

            for (int i = 0; i < computerShipCount; i++)
            {
                computerShips[i] = new Ship(reader.ReadInt32(), reader.ReadBoolean(), new Vector2(reader.ReadInt32(), reader.ReadInt32()));
            }

            reader.Close();

            playerBoard = new PlayerBoard(playerShips, playerWidth, playerHeight);
            computerBoard = new ComputerBoard(computerShips, computerWidth, computerHeight);
            playerBoard.shots = playerShots;
            computerBoard.shots = computerShots;
            GameLoop();
        }
        static void Instructions()
        {
            string text = BasicText(instructions, true);
            Console.Clear();
            Console.WriteLine(text);
            Console.ReadKey();
        }
        static void Quit() => Environment.Exit(0);
        static void Menu() {
            int option = 0;
            while (true) {
                string displayString = "";
                foreach (string s in title) {
                    displayString += new string(' ', centerX() - s.Length / 2) + s;
                }

                displayString += "\n\n";

                for (int i = 0; i < menuLength; i++) {
                    displayString += TextOption(menuOptions[i].display, option == i);
                }


                Console.Clear();
                Console.Write(displayString);
                switch (Console.ReadKey().Key) {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        option--;
                        option = (option + menuLength) % menuLength;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        option++;
                        option = (option + menuLength) % menuLength;
                        break;
                    case ConsoleKey.Enter:
                    case ConsoleKey.C:
                        menuOptions[option].function();
                        option = 0;
                        break;
                }
            }
        }

        static void GameLoop() {
            int selectingX = 0;
            int selectingY = 0;

            while (true) {
                BinaryWriter writer = new BinaryWriter(File.Open(currentSaveLocation, FileMode.Create));
                writer.Write(isPlayerTurn);
                writer.Write(playerBoard.width);
                writer.Write(playerBoard.height);
                writer.Write(computerBoard.width);
                writer.Write(computerBoard.height);
                writer.Write(playerBoard.ships.Length);
                writer.Write(computerBoard.ships.Length);

                for (int x = 0; x < playerBoard.width; x++) {
                    for (int y = 0; y < playerBoard.height; y++) {
                        writer.Write(playerBoard.shots[x, y]);
                    }
                }

                for (int x = 0; x < computerBoard.width; x++) {
                    for (int y = 0; y < computerBoard.height; y++) {
                        writer.Write(computerBoard.shots[x, y]);
                    }
                }

                foreach (Ship s in playerBoard.ships) {
                    writer.Write(s.length);
                    writer.Write(s.isVertical);
                    writer.Write((int)s.position.X);
                    writer.Write((int)s.position.Y);
                }

                foreach (Ship s in computerBoard.ships) {
                    writer.Write(s.length);
                    writer.Write(s.isVertical);
                    writer.Write((int)s.position.X);
                    writer.Write((int)s.position.Y);
                }

                writer.Close();

                if (isPlayerTurn) {
                    bool selecting = true;
                    bool hasMadeChange = true;
                    while (selecting) {
                        if (hasMadeChange) {
                            string text = GameDisplay(selectingX, selectingY, new string[] { $"Targeting {(char)(selectingY + 65)}{selectingX + 1} {(computerBoard.shots[selectingX, selectingY] ? " - Invalid target" : "")}", "Press enter to confirm" });
                            Console.Clear();
                            Console.WriteLine(text);
                            hasMadeChange = false;
                        }
                        switch (Console.ReadKey().Key) {
                            case ConsoleKey.Escape:
                                return;
                            case ConsoleKey.A:
                            case ConsoleKey.LeftArrow:
                                if (selectingX != 0) {
                                    selectingX--;
                                    hasMadeChange = true;
                                }
                                break;
                            case ConsoleKey.RightArrow:
                            case ConsoleKey.D:
                                if (selectingX != playerBoard.width - 1) {
                                    selectingX++;
                                    hasMadeChange = true;
                                }
                                break;
                            case ConsoleKey.W:
                            case ConsoleKey.UpArrow:
                                if (selectingY != 0) {
                                    selectingY--;
                                    hasMadeChange = true;
                                }
                                break;
                            case ConsoleKey.S:
                            case ConsoleKey.DownArrow:
                                if (selectingY != playerBoard.height - 1){
                                    selectingY++;
                                    hasMadeChange = true;
                                }
                                break;
                            case ConsoleKey.C:
                            case ConsoleKey.Enter:
                                if (!computerBoard.shots[selectingX, selectingY]) {
                                    selecting = false;
                                    computerBoard.shots[selectingX, selectingY] = true;
                                    string extraMessage = "You missed";
                                    Vector2 shotCoords = new Vector2(selectingX, selectingY);
                                    foreach (Ship ship in computerBoard.ships)
                                    {
                                        foreach (Vector2 position in ship.GetSquares())
                                        {
                                            if (shotCoords == position)
                                            {
                                                bool completelySunk = true;
                                                foreach (Vector2 v in ship.GetSquares())
                                                {
                                                    if (!computerBoard.shots[(int)v.X, (int)v.Y])
                                                    {
                                                        completelySunk = false;
                                                    }
                                                }

                                                if (completelySunk)
                                                {
                                                    extraMessage = "Hit and Sunk!";
                                                }
                                                else
                                                {
                                                    extraMessage = "Hit!";
                                                }
                                            }
                                        }
                                    }
                                    string text = GameDisplay(null, null, new string[] { $"Shot {(char)(selectingY + 65)}{selectingX + 1}; {extraMessage}", "Press any key to continue" });
                                    Console.Clear();
                                    Console.WriteLine(text);
                                    Console.ReadKey();
                                }
                                break;
                        }
                    }

                    if (computerBoard.hasLost()) {
                        File.Delete(currentSaveLocation);
                        string text = BasicText(new string[] { "You've won!", "Press any key to return to the menu" }, false);
                        Console.Clear();
                        Console.WriteLine(text);
                        Console.ReadKey();
                        return;
                    }
                } else {
                    bool hasMadeAttack = false;
                    int attackX = 0;
                    int attackY = 0;
                    while (!hasMadeAttack) {
                        attackX = random.Next(playerBoard.width);
                        attackY = random.Next(playerBoard.height);
                        if (!playerBoard.shots[attackX, attackY]) {
                            playerBoard.shots[attackX, attackY] = true;
                            hasMadeAttack = true;
                        }
                    }

                    string extraMessage = "They missed";
                    Vector2 target = new Vector2(attackX, attackY);
                    foreach (Ship ship in playerBoard.ships)
                    {
                        foreach (Vector2 v in ship.GetSquares())
                        {
                            if (target == v)
                            {
                                bool sunk = true;
                                foreach (Vector2 v1 in ship.GetSquares())
                                {
                                    if (!playerBoard.shots[(int)v1.X, (int)v1.Y])
                                    {
                                        sunk = false;
                                    }
                                }

                                if (sunk)
                                {
                                    extraMessage = "They hit and sunk one of your ships!";
                                }
                                else
                                {
                                    extraMessage = "They hit one of your ships!";
                                }
                            }
                        }
                    }
                    string[] messages = {$"Computer shot {(char)(attackY + 65)}{attackX + 1}; {extraMessage}", "Press any key to continue"};
                    string text = GameDisplay(null, null, messages);
                    Console.Clear();
                    Console.WriteLine(text);
                    Console.ReadKey();

                    if (playerBoard.hasLost())
                    {
                        File.Delete(currentSaveLocation);
                        text = BasicText(new string[] { "You've lost!", "Press any key to return to the menu" }, false);
                        Console.Clear();
                        Console.WriteLine(text);
                        Console.ReadKey();
                        return;
                    }
                }
                isPlayerTurn = !isPlayerTurn;
            }
        }

        static string BasicText(string[] lines, bool uniformStart)
        {
            string returnString = "";
            foreach (string s in title)
            {
                returnString += new string(' ', centerX() - s.Length / 2) + s;
            }
            returnString += "\n";
            int maxLength = 0;
            foreach (string s in lines)
            {
                if (s.Length > maxLength)
                {
                    maxLength = s.Length;
                }
            }

            int i = 0;

            foreach (string s in lines)
            {
                i++;
                if (uniformStart)
                {
                    returnString += new string(' ', centerX() - maxLength / 2);
                } else
                {
                    returnString += new string(' ', centerX() - s.Length / 2);
                }

                returnString += s;
                if (i !=  lines.Length)
                {
                    returnString += "\n";
                }
            }
            return returnString;
        }
        static string GameDisplay(int? selectedX, int? selectedY, string[] messages)
        {
            string displayString = "";

            foreach (string s in title)
            {
                displayString += new string(' ', centerX() - s.Length / 2) + s;
            }

            displayString += "\n\n";

            int maxHeight = Math.Max(playerBoard.height, computerBoard.height);
            int startX = centerX() - boardGap / 2 - playerBoard.width * 2;
            
            for (int y = 0; y < maxHeight; y++)
            {
                displayString += new string(' ', startX);
                if (y < playerBoard.height)
                {
                    displayString += playerBoard.getDisplayLine(y);
                } else
                {
                    displayString += new string(' ', playerBoard.width * 2);
                }

                if (y < computerBoard.height)
                {
                    displayString += new string(' ', boardGap);
                    displayString += computerBoard.getDisplayLine(y, selectedX, selectedY);
                }
                
                displayString += "\n";
            }

            displayString += "\n\n";
            foreach (string message in messages)
            {
                displayString += new string(' ', centerX() - message.Length / 2);
                displayString += message;
                displayString += '\n';
            }

            return displayString;
        }
        static string PlacingDisplay(int width, int height, List<Ship> ships, int currentX, int currentY, int currentLength, bool isVertical, List<int> shipsToPlace, int currentShipIndex) {
            string returnString = "";
            bool targettingShip = false;

            foreach (string s in title) {
                returnString += new string(' ', centerX() - s.Length / 2) + s;
            }

            returnString += "\n";
            int startGrid = centerX() - width;

            for (int y = 0; y < height; y++) {
                returnString += new string(' ', startGrid);
                for (int x = 0; x < width; x++) {
                    bool isShip = false;
                    foreach (Ship s in ships) {
                        foreach (Vector2 v in s.GetSquares())
                        {
                            if ((int)v.X == x && (int)v.Y == y) { isShip = true; }
                        }
                    }

                    if (isVertical) {
                        if (x == currentX && y >= currentY && y < currentY + currentLength) {
                            returnString += "# ";
                            if (isShip)
                            {
                                targettingShip = true;
                            }
                        }
                        else
                        {
                            if (isShip)
                            {
                                returnString += "B ";
                            }
                            else
                            {
                                returnString += ". ";
                            }
                        }
                    } else {
                        if (y == currentY && x >= currentX && x < currentX + currentLength)
                        {
                            returnString += "# ";
                            if (isShip)
                            {
                                targettingShip = true;
                            }
                        }
                        else
                        {
                            if (isShip)
                            {
                                returnString += "B ";
                            }
                            else
                            {
                                returnString += ". ";
                            }
                        }
                    }
                }
                returnString += "\n";
            }

            string targetingMessage = $"Placing at {(char)(currentY + 65)}{currentX + 1} - {(targettingShip ? "invalid target" : "press enter to place")}";
            returnString += "\n" + new string(' ', centerX() - targetingMessage.Length / 2) + targetingMessage;
            returnString += "\n";

            int maximumShipLength = 0;
            foreach (int i in shipsToPlace)
            {
                if (i > maximumShipLength)
                {
                    maximumShipLength = i;
                }
            }

            for (int i = 0; i < maximumShipLength; i++)
            {
                returnString += "\n" + new string(' ', centerX() - shipsToPlace.Count);
                for (int j = 0; j < shipsToPlace.Count; j++)
                {
                    if (shipsToPlace[j] > i)
                    {
                        if (j == currentShipIndex)
                        {
                            returnString += "# ";
                        }
                        else
                        {
                            returnString += "| ";
                        }
                    }
                    else
                    {
                        returnString += "  ";
                    }
                }
            }
            return returnString;
        }
        static string TextOption(string text, bool selected)
        {
            int width = 30;
            int height = 5;
            int textStart = width/2 - text.Length / 2;

            char verticalChar = '|';
            char horizontalChar = '-';

            if (selected)
            {
                verticalChar = '#';
                horizontalChar = '#';
            }

            string returnString = "";
            for (int y = 0; y < height; y++)
            {
                returnString += new string(' ', centerX() - width / 2);
                for (int x = 0; x < width; x++)
                {
                    if (y == 0 || y == height - 1)
                    {
                        returnString += horizontalChar;
                    }
                    else if (x == 0 || x == width - 1)
                    {
                        returnString += verticalChar;
                    }
                    else if (y == height / 2 && x >= textStart)
                    {
                        if (x - textStart < text.Length)
                        {
                            returnString += text[x - textStart];
                        } else {
                            returnString += " ";
                        }
                    } else {
                        
                        returnString += " ";
                    }
                }
                returnString += "\n";
            }
            return returnString;
        }
        static int centerX() {return Console.WindowWidth / 2;}

        struct Option
        {
            public Option(Action function, string display)
            {
                this.function = function;
                this.display = display;
            }

            public Action function;
            public string display;
        }
        class Board
        {
            public Board(Ship[] ships, int width, int height)
            {
                shots = new bool[width, height];
                this.ships = ships;
                this.width = width;
                this.height = height;
            }

            public bool hasLost()
            {
                foreach (Ship s in ships)
                {
                    foreach (Vector2 v in s.GetSquares())
                    {
                        if (!shots[(int)v.X, (int)v.Y])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public int width, height;
            public bool[,] shots;
            public Ship[] ships;
        }
        class PlayerBoard : Board
        {
            public PlayerBoard(Ship[] ships, int width, int height) : base(ships, width, height) {}

            public string getDisplayLine(int y)
            {
                string returnString = "";
                for (int x = 0; x < width; x++)
                {
                    Vector2 position = new Vector2(x, y);
                    bool isShip = false;
                    foreach (Ship ship in ships)
                    {
                        foreach (Vector2 v in ship.GetSquares())
                        {
                            if (position == v)
                            {
                                isShip = true;
                            }
                            
                        }
                    }
                    if (isShip)
                    {
                        if (shots[x, y])
                        {
                            returnString += "H ";
                        } else
                        {
                            returnString += "B ";
                        }
                    } else
                    {
                        returnString += ". ";
                    }
                }
                return returnString;
            }
        }
        class ComputerBoard : Board
        {
            public ComputerBoard(Ship[] ships, int width, int height) : base(ships, width, height) {}
            public string getDisplayLine(int y, int? selectedX, int? selectedY)
            {
                if (y >= height)
                {
                    return new string(' ', width);
                }
                string returnString = "";

                for (int x = 0; x < width; x++) {
                    bool isShip = false;
                    Vector2 position = new Vector2(x, y);
                    foreach (Ship ship in ships)
                    {
                        foreach (Vector2 v in ship.GetSquares())
                        {
                            if (position == v) {
                                isShip = true;
                            }
                        }
                    }
                    if (selectedX != null && y == selectedY && x == selectedX) {
                        returnString += "# ";
                    } else if (shots[x, y])
                    {
                        if (isShip) {
                            returnString += "H ";
                        } else {
                            returnString += "M ";
                        }
                    } else {
                        if (isShip) {
                            returnString += ". ";
                        } else {
                            returnString += ". ";
                        }
                    }
                }
                return returnString;
            }
        }
        struct Ship
        {
            public Ship(int length, bool isVertical, Vector2 position)
            {
                this.length = length;
                this.isVertical = isVertical;
                this.position = position;
            }

            public Vector2[] GetSquares()
            {
                Vector2[] returnVectors = new Vector2[length];
                for (int i = 0; i < length; i++)
                {
                    if (isVertical)
                    {
                        returnVectors[i] = position + i * Vector2.UnitY;
                    } else
                    {
                        returnVectors[i] = position + i * Vector2.UnitX;
                    }
                }
                return returnVectors;
            }

            public int length;
            public bool isVertical;
            public Vector2 position;
        }
    }
}
