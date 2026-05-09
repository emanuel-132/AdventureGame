namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";

	private Adventurer adventurer;
	private Room[,] dungeon;
	private int aRow;
	private int aCol;

	private int grueRow;
  private int grueCol;
  private bool isGrueActive;

  private int exitRow; 
  private int exitCol;

	private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool isAdventureAlive;
	private string lastDirection;

	public AdventureGame()
	{

	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while(!IsValidInput(input));

			ProcessInput(input);

			UpdateGameState();
		}
		while(!IsGameOver());

		ShowGameOverScreen();
	}

private void Init()
  {
    adventurer = new Adventurer();

    // Ruta correcta 
    string mapPath = "../../res/DungeonTemplate.txt";

    // 1. Se carga el mapa con el tool que hizo el profesor 
    dungeon = DungeonLoader.Load(mapPath);

    // 2. Se lee las coordenadas de la salida/entrada para saber dónde empieza el jugador
   string[] lines = File.ReadAllLines(mapPath);
    exitRow = int.Parse(lines[2]); // Guarda la fila de salida
    exitCol = int.Parse(lines[3]); // Guarda la columna de salida
    aRow = exitRow; // El jugador empieza en la salida
    aCol = exitCol;
		// El Grue empieza en las coordenadas del archivo y dormido
    grueRow = int.Parse(lines[10]);
    grueCol = int.Parse(lines[11]);
    isGrueActive = false;

    // 3. Variables de estado del juego
    isChestOpen = false;
    hasPlayerQuit = false;
    isAdventureAlive = true;

    lastDirection = string.Empty;
  }

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		var r = dungeon[aRow, aCol];

		if(adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if(!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if(!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			Console.WriteLine("You got eaten alive by the Grue!");
			isAdventureAlive = false;
		}
		else if(input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if(input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if(input == GO_EAST)
		{
			GoEast(r);
		}
		else if(input == GO_WEST)
		{
			GoWest(r);
		}
		else if(input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if(input == GET_KEY)
		{
			GetKey(r);
		}
		else if(input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}
	}

	private void UpdateGameState()
  {
    // Solo se mueve si el Grue está activo y el jugador sigue vivo
    if (isGrueActive && isAdventureAlive && !(aRow == exitRow && aCol == exitCol))
    {
      MoveGrueTowardsPlayer();
      
      if (grueRow == aRow && grueCol == aCol)
      {
        Console.WriteLine("\nOH NO! The Grue caught you! You have been eaten!");
        isAdventureAlive = false;
      }
      else
      {
        Console.WriteLine("\n[!] You hear heavy footsteps... The Grue is now at Row " + grueRow + ", Col " + grueCol);
      }
    }
  }

  // ALGORITMO BFS
  private void MoveGrueTowardsPlayer()
  {
    if (grueRow == aRow && grueCol == aCol) return;

    int rows = dungeon.GetLength(0);
    int cols = dungeon.GetLength(1);
    
    // Para rastrear cuartos visitados y de dónde venimos (el camino)
    bool[,] visited = new bool[rows, cols];
    (int r, int c)[,] parent = new (int, int)[rows, cols];
    
    Queue<(int r, int c)> queue = new Queue<(int r, int c)>();
    
    // Iniciar BFS desde el Grue
    queue.Enqueue((grueRow, grueCol));
    visited[grueRow, grueCol] = true;
    
    bool foundPlayer = false;
    
    while(queue.Count > 0)
    {
      var curr = queue.Dequeue();
      
      // Si el BFS encuentra al jugador, rompemos ciclo
      if (curr.r == aRow && curr.c == aCol)
      {
        foundPlayer = true;
        break;
      }
      
      Room currRoom = dungeon[curr.r, curr.c];
      
      // Revisar Norte
      if (currRoom.HasNorth() && !visited[curr.r - 1, curr.c]) {
        visited[curr.r - 1, curr.c] = true;
        parent[curr.r - 1, curr.c] = curr;
        queue.Enqueue((curr.r - 1, curr.c));
      }
      // Revisar Sur
      if (currRoom.HasSouth() && !visited[curr.r + 1, curr.c]) {
        visited[curr.r + 1, curr.c] = true;
        parent[curr.r + 1, curr.c] = curr;
        queue.Enqueue((curr.r + 1, curr.c));
      }
      // Revisar Este
      if (currRoom.HasEast() && !visited[curr.r, curr.c + 1]) {
        visited[curr.r, curr.c + 1] = true;
        parent[curr.r, curr.c + 1] = curr;
        queue.Enqueue((curr.r, curr.c + 1));
      }
      // Revisar Oeste
      if (currRoom.HasWest() && !visited[curr.r, curr.c - 1]) {
        visited[curr.r, curr.c - 1] = true;
        parent[curr.r, curr.c - 1] = curr;
        queue.Enqueue((curr.r, curr.c - 1));
      }
    }
    
    // Si se encuentra una ruta, trazamos hacia atrás para encontrar el primer paso
    if (foundPlayer)
    {
      var step = (r: aRow, c: aCol);
      
      while (parent[step.r, step.c] != (grueRow, grueCol))
      {
        step = parent[step.r, step.c];
      }
      
      // Movemos al Grue a ese cuarto
      grueRow = step.r;
      grueCol = step.c;
    }
  }

private bool IsGameOver()
  {
    // Ganas si abriste el cofre Y estás parado exactamente en la salida
    bool hasWon = isChestOpen && (aRow == exitRow && aCol == exitCol);
    
    // El juego acaba si ganas, si haces quit, o si mueres
    return hasWon || hasPlayerQuit || !isAdventureAlive;
  }

private void ShowGameOverScreen()
  {
    if (isChestOpen && aRow == exitRow && aCol == exitCol)
    {
        Console.WriteLine("CONGRATULATIONS! You escaped with the treasure and survived the Grue!");
    }
    else
    {
        Console.WriteLine("Game Over!");
    }
  }

	private void GoNorth(Room r)
	{
		if(r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if(r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if(r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if(r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if(r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if(r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
  {
    if(r.HasChest())
    {
      if(adventurer.HasKey())
      {
        isChestOpen = true;
				isGrueActive = true; 
        Console.WriteLine("You got the treasure! Suddenly, you hear a terrifying roar...");
        Console.WriteLine("The Grue is awake! RUN TO THE EXIT!");
      }
      else
      {
        Console.WriteLine("You do not have the key!");
      }
    }
    else
    {
      Console.WriteLine("There is no chest in this room.");
    }
  }

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}
}
