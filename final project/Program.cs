using System;
using System.IO;
using System.Collections.Generic;

namespace AirBnB
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual void ViewDetails()
        {
            Console.WriteLine($"Name: {Name}");
        }
    }


    public class Room : BaseEntity
    {
        public string RoomType { get; set; }

        public Guest Guest { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = "Available"; 

        public Room(int id, string roomType, decimal price, int capacity)
        {
            Id = id;
            RoomType = roomType;
            Price = price;
            Capacity = capacity;
        }

        public void ViewStatus()
        {
            Console.WriteLine($"Room {Id} ({RoomType}): {Status} | Capacity: {Capacity} pax |  - Price: {Price:C},");
        }

        public void ChangeStatus(string status)
        {
            Status = status;
        }

        public override void ViewDetails()
        {
            base.ViewDetails();
            Console.WriteLine($"Room Type: {RoomType}, Status: {Status}, Price: {Price:C}");
        }
    }


    public class Guest : BaseEntity
    {
        public int RoomNumber { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }

        public Guest(string name, int roomNumber, string checkInDate, string checkOutDate)
        {
            Name = name;
            RoomNumber = roomNumber;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }

        public void GenerateReceipt(decimal roomPrice)
        {
            try
            {
                DateTime checkIn = ParseOrPromptDate(CheckInDate, "check-in");
                DateTime checkOut = ParseOrPromptDate(CheckOutDate, "check-out");

                if (checkOut < checkIn)
                {   
                    Console.WriteLine("Error: Check-out date cannot be earlier than check-in date.");
                    return;
                }

                TimeSpan duration = checkOut - checkIn;
                decimal totalPrice = roomPrice * duration.Days;

                Console.WriteLine("\nReceipt:");
                Console.WriteLine($"Guest Name: {Name}");
                Console.WriteLine($"Room: {RoomNumber}");
                Console.WriteLine($"Check-in Date: {checkIn:yyyy-MM-dd}");
                Console.WriteLine($"Check-out Date: {checkOut:yyyy-MM-dd}");
                Console.WriteLine($"Duration of Stay: {duration.Days} days");
                Console.WriteLine($"Total Price: {totalPrice:C}");
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"An error occurred while generating the receipt: {ex.Message}");
            }
        }

        private DateTime ParseOrPromptDate(string dateString, string dateType)
        {
            DateTime date;
            while (!DateTime.TryParse(dateString, out date))
            {
                Console.Write($"Invalid {dateType} date. Please enter a valid {dateType} date (YYYY-MM-DD):");
                dateString = Console.ReadLine();
            }
            return date;
        }

        public override void ViewDetails()
        {
            base.ViewDetails();
            Console.WriteLine($"Room Number: {RoomNumber}, Check-in: {CheckInDate}, Check-out: {CheckOutDate}");
        }
    }



    public class System
    {
        private List<Room> rooms = new List<Room>();
        private List<Guest> guests = new List<Guest>();

        private string guestFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Guests Data.txt");


        public System()
        {
            
            rooms.Add(new Room(1, "Single", 100, 1));
            rooms.Add(new Room(2, "Single", 100, 1));
            rooms.Add(new Room(3, "Double", 230, 2));
            rooms.Add(new Room(4, "Double", 230, 2));
            rooms.Add(new Room(5, "Deluxe", 520, 3));
            rooms.Add(new Room(6, "Deluxe", 520, 3));
            rooms.Add(new Room(7, "Family", 800, 5));

            LoadGuestsFromFile();
        }


        public void SaveGuestsToFile()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(guestFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter writer = new StreamWriter(guestFilePath))
                {
                    foreach (var guest in guests)
                    {
                        var room = rooms.FirstOrDefault(r => r.Id == guest.RoomNumber);
                        string roomStatus = room != null ? room.Status : "Unknown";
                        writer.WriteLine($"{guest.Name}|{guest.RoomNumber}|{guest.CheckInDate}|{guest.CheckOutDate}|{roomStatus}");


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving guests to file: {ex.Message}");
            }
        }


        public void LoadGuestsFromFile()
        {
            try
            {
                if (File.Exists(guestFilePath))
                {
                    using (StreamReader reader = new StreamReader(guestFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var guestData = line.Split('|');
                            if (guestData.Length == 5)
                            {
                                string guestName = guestData[0];
                                int roomNumber = int.Parse(guestData[1]);
                                string savedRoomStatus = guestData[4];

                                guests.Add(new Guest(guestName, roomNumber, guestData[2], guestData[3]));

                                
                                Room room = FindRoom(roomNumber);
                                if (room != null)
                                {
                                    room.ChangeStatus(savedRoomStatus);
                                }
                                else
                                {
                                    Console.WriteLine($"Warning: Room {roomNumber} from file not found in system.");
                                }
                            }
                        }
                    }

                }
                else
                {
                    Console.WriteLine("No existing guest data found. Starting fresh.");
                }
            }
            catch (Exception ex)
            {          
                Console.WriteLine($"Error loading guests from file: {ex.Message}");    
            }
        }


        public void UpdateGuestFile()
        {
            SaveGuestsToFile();
        }


        public void ReserveRoom()
        {
            bool keepReserving = true;

            while (keepReserving)
            {
                Console.Clear();
                Console.WriteLine("Available Rooms:");

                bool anyAvailableRoom = false;
                foreach (var room in rooms)
                {
                    if (room.Status == "Available")
                    {
                        room.ViewStatus();
                        anyAvailableRoom = true;
                    }
                }

                if (!anyAvailableRoom)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nNo rooms are available for reservation at the moment.\n");
                    Console.ResetColor();
                    return;
                }

                Console.Write("\nEnter room number to reserve: ");
                if (int.TryParse(Console.ReadLine(), out int roomNumber))
  
                {   
                    Room roomToReserve = FindRoom(roomNumber);
                    if (roomToReserve != null && roomToReserve.Status == "Available")
                    {
                        Console.Write("\nEnter guest name: ");
                        string guestName = Console.ReadLine();

                        DateTime checkInDate;
                        while (true)
                        {
                            Console.Write("\nEnter check-in date (yyyy-mm-dd): ");
                            string checkInDateInput = Console.ReadLine();

                            if (DateTime.TryParse(checkInDateInput, out checkInDate))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("\nInvalid date format. Please try again.");
                            }
                        }

                        DateTime checkOutDate;
                        while (true)
                        {
                            Console.Write("Enter check-out date (yyyy-mm-dd): ");
                            string checkOutDateInput = Console.ReadLine();

                            if (DateTime.TryParse(checkOutDateInput, out checkOutDate))
                            {
                                if (checkOutDate > checkInDate)
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("\nCheck-out date must be later than check-in date. Please try again.\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid date format. Please try again.");
                            }
                        }

                        
                        TimeSpan stayDuration = checkOutDate - checkInDate;
                        decimal totalAmount = roomToReserve.Price * stayDuration.Days;              
                        decimal deposit = totalAmount * 0.20M;
                        Console.Clear();
                        Console.WriteLine($"\nThe total cost for the stay is {totalAmount:C} for {stayDuration.Days} nights.");
                        Console.WriteLine($"\nTo reserve the room, a deposit of {deposit:C} is required.");
                        Console.Write("\nDo you confirm to pay the deposit? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            roomToReserve.ChangeStatus("Reserved");
                            Console.Clear();

                            guests.Add(new Guest(guestName, roomNumber, checkInDate.ToString("yyyy-MM-dd"), checkOutDate.ToString("yyyy-MM-dd")));
                            UpdateGuestFile();

                            Console.WriteLine($"\nRoom {roomToReserve.Id} ({roomToReserve.RoomType}) has been reserved for {guestName}. \n");
                        }
                        else
                        {
                            Console.WriteLine("\nReservation canceled due to lack of deposit.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nRoom {roomNumber} is not available for reservation.\n");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid room number.\n");
                }

                Console.Write("\nWould you like to reserve another room? (y/n): ");
                string userInput = Console.ReadLine()?.ToLower();
                if (userInput != "y")
                {
                    keepReserving = false;
                }
            }
        }



        public void ViewRoomOptions()
        {
            bool validChoice = false;

            while (!validChoice)
            {
                Console.Clear();
                Console.WriteLine("View Room Options:");
                Console.WriteLine("1. View All Rooms");
                Console.WriteLine("2. Search Room by Number");
                Console.WriteLine("3. Change Room Status");
                Console.WriteLine("4. Return to Main Menu");
                Console.Write("\nSelect an option: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": // View all rooms
                            DisplayAllRooms();
                            break;

                        case "2": // Search for a room
                            Console.Write("Enter room number to search: ");
                            if (int.TryParse(Console.ReadLine(), out int roomNumber))
                            {
                                SearchRoom(roomNumber);
                            }
                            else
                            {
                                Console.WriteLine("Invalid room number. Please enter a valid integer.");
                            }
                            break;

                        case "3": // Change room status
                            Console.WriteLine("\nEmergency status (Under Maintenance). If fixed, change to (Available).");
                            Console.Write("\nEnter room number to change status: ");
                            if (int.TryParse(Console.ReadLine(), out roomNumber))
                            {
                                Console.Write("Enter new status: ");
                                string status = Console.ReadLine();
                                ChangeRoomStatus(roomNumber, status);
                            }
                            else
                            {
                                Console.WriteLine("Invalid room number. Please enter a valid integer.");
                            }
                            break;

                        case "4": // Return to main menu
                            validChoice = true;
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                if (!validChoice)
                {
                    Console.WriteLine("\nPress any key to return to the options menu...");
                    Console.ReadKey();
                }
            }
        }


        public void DisplayAllRooms()
        {
            bool anyRoomDisplayed = false;

            foreach (var room in rooms)
            {
                room.ViewStatus();
                anyRoomDisplayed = true;
            }

            if (!anyRoomDisplayed)
            {
                Console.WriteLine("No rooms to display.");
            }
            UpdateGuestFile();
        }


        public void SearchRoom(int roomNumber)
        {
            Room room = FindRoom(roomNumber);
            if (room != null)
            {
                room.ViewStatus();
            }
            else
            {
                Console.WriteLine($"Room {roomNumber} not found.");
            }
            UpdateGuestFile();
        }


        public void ChangeRoomStatus(int roomNumber, string newStatus)
        {
            Room room = FindRoom(roomNumber);
            if (room != null)
            {
                room.ChangeStatus(newStatus);
                Console.WriteLine($"Room {roomNumber} status changed to {newStatus}.");
            }
            else
            {
                Console.WriteLine($"Room {roomNumber} not found.");
            }
            UpdateGuestFile();
        }


        public void CheckIn()
        {
            bool keepCheckingIn = true;

            while (keepCheckingIn)
            {
                Console.Clear();
                Console.WriteLine("Reserved Guests:");
                foreach (var reservedGuest in guests.Where(g => FindRoom(g.RoomNumber)?.Status == "Reserved"))
                {
                    Console.WriteLine($"Name: {reservedGuest.Name}, Room: {reservedGuest.RoomNumber}, Check-In: {reservedGuest.CheckInDate}");
                }

                Console.Write("\nEnter guest name to check in: ");
                string guestName = Console.ReadLine();

                Guest foundGuest = FindGuestByName(guestName);
                if (foundGuest != null)
                {
                    Room room = FindRoom(foundGuest.RoomNumber);
                    if (room != null)
                    {
                        room.ChangeStatus("Occupied");
                        Console.WriteLine($"{foundGuest.Name} has checked in successfully!");
                        UpdateGuestFile();
                    }
                    else
                    {
                        Console.WriteLine("Room not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Guest not found.");
                }

                Console.Write("\nWould you like to check in another guest? (y/n): ");
                string userInput = Console.ReadLine()?.ToLower();
                if (userInput != "y")
                {
                    keepCheckingIn = false;
                }
            }
            UpdateGuestFile();
        }


        public void CheckOut()
        {
            Console.Clear();
            bool keepCheckingOut = true;
            string checkOutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CheckedOutGuests.txt");
            
            while (keepCheckingOut)
            {
                Console.WriteLine("Guests Ready for Check-Out:");
                foreach (var checkedInGuest in guests.Where(g => FindRoom(g.RoomNumber)?.Status == "Occupied"))
                {
                    Console.WriteLine($"Name: {checkedInGuest.Name}, Room: {checkedInGuest.RoomNumber}, Check-Out: {checkedInGuest.CheckOutDate}");
                }

                Console.Write("\nEnter guest name to check out: ");
                string guestName = Console.ReadLine();
                Guest foundGuest = FindGuestByName(guestName);

                if (foundGuest != null)
                {
                    Room room = FindRoom(foundGuest.RoomNumber);
                    if (room != null)
                    {
                        decimal totalAmount = room.Price * (decimal)(DateTime.Parse(foundGuest.CheckOutDate) - DateTime.Parse(foundGuest.CheckInDate)).Days;
                        decimal balance = totalAmount * 0.8M;

                        Console.WriteLine($"\nRemaining balance for the stay: {balance:C}");
                        Console.WriteLine("\nPayment methods available: 1. Cash  2. Credit Card  3. Mobile Payment");
                        Console.Write("\nChoose a payment method (1/2/3): ");
                        string paymentChoice = Console.ReadLine();

                        Console.Write("Enter payment amount: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal payment) && payment >= balance)
                        {
                            room.ChangeStatus("Available");
                            foundGuest.GenerateReceipt(room.Price);

                            SaveCheckedOutGuestData(checkOutFilePath, foundGuest);

                            guests.Remove(foundGuest);
                            SaveGuestsToFile();
                            Console.WriteLine($"{foundGuest.Name} has checked out successfully!");
                        }
                        else
                        {
                            Console.WriteLine("Payment insufficient or invalid. Please try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Room not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Guest not found.");
                }

                Console.Write("\nWould you like to check out another guest? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y") keepCheckingOut = false;
            }
            SaveGuestsToFile();
        }


        private void SaveCheckedOutGuestData(string filePath, Guest guest)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{guest.Name}|{guest.RoomNumber}|{guest.CheckInDate}|{guest.CheckOutDate}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving guest data: {ex.Message}");
            }
        }



        private Room FindRoom(int roomNumber)
        {
            return rooms.Find(room => room.Id == roomNumber);
        }


        private Guest FindGuestByName(string name)
        {
            return guests.Find(guest => guest.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        public void ManageGuestsMenu()
        {
            bool validChoice = false;

            while (!validChoice)
            {
                Console.Clear();
                Console.WriteLine("Guest Management Options:");
                Console.WriteLine("1. View All Guests");
                Console.WriteLine("2. View Specific Guest");
                Console.WriteLine("3. Update Guest");
                Console.WriteLine("4. Delete Guest");
                Console.WriteLine("5. Return to Main Menu");
                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": 
                        ViewAllGuests();
                        break;

                    case "2": 
                        Console.Write("Enter the guest name: ");
                        string guestName = Console.ReadLine();
                        ViewGuest(guestName);
                        UpdateGuestFile();
                        break;

                    case "3": 
                        ViewAllGuests();
                        Console.Write("\nEnter the guest name to update: ");
                        guestName = Console.ReadLine();
                        UpdateGuest(guestName);
                        UpdateGuestFile();
                        break;
                        

                    case "4": 
                        ViewAllGuests();
                        Console.Write("\nEnter the guest name to delete: ");
                        guestName = Console.ReadLine();
                        DeleteGuest(guestName);
                        UpdateGuestFile();
                        break;

                    case "5": 
                        validChoice = true;
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                if (!validChoice)
                {
                    Console.WriteLine("\nPress any key to return to the options menu...");
                    Console.ReadKey();
                }
            }
            UpdateGuestFile();
        }


        private Room FindRoomByGuest(Guest guest)
        {
            return rooms.FirstOrDefault(r => r.Guest == guest); // Replace 'rooms' with your room collection
        }


        public void ViewAllGuests()
        {
            if (guests.Count == 0)
            {
                Console.WriteLine("\nNo guests are currently in the system.");
                return;
            }

            Console.WriteLine("\nGuests currently in the system:");
            foreach (var guest in guests)
            {
                Console.WriteLine($"Name: {guest.Name}, Room: {guest.RoomNumber}, Check-In: {guest.CheckInDate}, Check-Out: {guest.CheckOutDate}");
            }
            UpdateGuestFile();
        }


        public void ViewGuest(string guestName)
        {
            Guest guest = FindGuestByName(guestName);
            if (guest != null)
            {
                guest.ViewDetails();
            }
            else
            {
                Console.WriteLine("Guest not found.");
            }
            Console.ReadKey();
            UpdateGuestFile();
        }


        public void UpdateGuest(string guestName)
        {
            Console.Clear();    
            Guest guest = FindGuestByName(guestName);
            if (guest != null)
            {
                Console.WriteLine($"\nUpdating information for {guestName}:");
                Console.WriteLine("1. Update Guest Name");
                Console.WriteLine("2. Update Room Number");
                Console.WriteLine("3. Update Check-In Date");
                Console.WriteLine("4. Update Check-Out Date");
                Console.WriteLine("5. Cancel");
                Console.Write("\nSelect an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        
                        Console.Write("Enter new name for the guest: ");
                        string newName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            guest.Name = newName;
                            Console.WriteLine($"\nGuest name updated to {newName}.");
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid name. No changes made.");
                        }
                        break;

                    case "2":
                        // Update Room Number
                        Console.Write("Enter new room number: ");
                        if (int.TryParse(Console.ReadLine(), out int newRoomNumber))
                        {
                            Room newRoom = FindRoom(newRoomNumber);
                            if (newRoom != null && newRoom.Status != "Occupied")
                            {
                                guest.RoomNumber = newRoomNumber;
                                Console.WriteLine($"Room number updated to {newRoomNumber}.");
                            }
                            else
                            {
                                Console.WriteLine("Invalid room number or room is already occupied.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid room number format.");
                        }
                        break;

                    case "3":
                        // Update Check-In Date
                        Console.Write("Enter new check-in date (yyyy-mm-dd): ");
                        string newCheckInDate = Console.ReadLine();
                        if (DateTime.TryParse(newCheckInDate, out DateTime checkInDate))
                        {
                            guest.CheckInDate = checkInDate.ToString("yyyy-MM-dd");
                            Console.WriteLine($"Check-in date updated to {checkInDate:yyyy-mm-dd}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid date format.");
                        }
                        break;

                    case "4":
                        // Update Check-Out Date
                        Console.Write("Enter new check-out date (yyyy-mm-dd): ");
                        string newCheckOutDate = Console.ReadLine();
                        if (DateTime.TryParse(newCheckOutDate, out DateTime checkOutDate))
                        {
                            guest.CheckOutDate = checkOutDate.ToString("yyyy-MM-dd");
                            Console.WriteLine($"Check-out date updated to {checkOutDate:yyyy-mm-dd}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid date format.");
                        }
                        break;

                    case "5":
                        // Cancel Update
                        Console.WriteLine("Update canceled.");
                        return;

                    default:
                        Console.WriteLine("Invalid option. No changes made.");
                        return;
                }

                SaveGuestsToFile();
                Console.WriteLine("Guest information updated successfully.");
            }
            else
            {
                Console.WriteLine("Guest not found.");
            }
            Console.ReadKey();
            UpdateGuestFile();
        }


        public void DeleteGuest(string guestName)
        {
           
            Guest guest = FindGuestByName(guestName);
            if (guest != null)
            {
                Room room = FindRoomByGuest(guest); 
                if (room != null)
                {
                    room.Status = "Available"; 
                }

                guests.Remove(guest);
                SaveGuestsToFile(); 
                Console.WriteLine($"{guestName} has been deleted and their room is now available.");
                UpdateGuestFile();
            }
            else
            {
                Console.WriteLine("Guest not found.");
                UpdateGuestFile();
            }

            UpdateGuestFile(); 
            Console.ReadKey();
        }


        public void GenerateSalesReport()
        {
            Console.Clear();
            Console.WriteLine("Sales Report Options:");
            Console.WriteLine("1. Weekly Report");
            Console.WriteLine("2. Monthly Report");
            Console.WriteLine("3. Yearly Report");
            Console.Write("\nSelect an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    GenerateReportForPeriod(DateTime.Now.AddDays(-7), DateTime.Now, "Weekly Sales Report");
                    break;
                case "2":
                    GenerateReportForPeriod(DateTime.Now.AddMonths(-1), DateTime.Now, "Monthly Sales Report");
                    break;
                case "3":
                    GenerateReportForPeriod(DateTime.Now.AddYears(-1), DateTime.Now, "Yearly Sales Report");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Returning to menu...");
                    break;
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void GenerateReportForPeriod(DateTime startDate, DateTime endDate, string reportTitle)
        {
            Console.Clear();
            Console.WriteLine($"{reportTitle}:");
            decimal totalSales = 0;

            // Combine guest data from memory and file
            var allGuests = new List<Guest>(guests);
            allGuests.AddRange(LoadCheckedOutGuestsFromFile());

            foreach (var guest in allGuests)
            {
                Room room = FindRoom(guest.RoomNumber);
                if (room != null &&
                    DateTime.TryParse(guest.CheckInDate, out DateTime checkInDate) &&
                    DateTime.TryParse(guest.CheckOutDate, out DateTime checkOutDate))
                {
                    // Determine overlapping period
                    DateTime effectiveStartDate = checkInDate > startDate ? checkInDate : startDate;
                    DateTime effectiveEndDate = checkOutDate < endDate ? checkOutDate : endDate;

                    if (effectiveStartDate <= effectiveEndDate)
                    {
                        TimeSpan stayDuration = effectiveEndDate - effectiveStartDate;
                        decimal revenue = room.Price * (stayDuration.Days + 1); // Include the checkout day
                        totalSales += revenue;

                        Console.WriteLine($"Guest: {guest.Name}, Room: {guest.RoomNumber}, Revenue: {revenue:C}, Check-In: {checkInDate:yyyy-MM-dd}, Check-Out: {checkOutDate:yyyy-MM-dd}");
                    }
                }
            }

            Console.WriteLine($"\nTotal {reportTitle}: {totalSales:C}");
        }

        private List<Guest> LoadCheckedOutGuestsFromFile()
        {
            var checkedOutGuests = new List<Guest>();
            string checkOutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CheckedOutGuests.txt");

            try
            {
                if (File.Exists(checkOutFilePath))
                {
                    using (StreamReader reader = new StreamReader(checkOutFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var guestData = line.Split('|');
                            if (guestData.Length == 4)
                            {
                                checkedOutGuests.Add(new Guest(guestData[0], int.Parse(guestData[1]), guestData[2], guestData[3]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading checked-out guest data: {ex.Message}");
            }

            return checkedOutGuests;
        }




    }

    class Program
    {
        static void Main(string[] args)
        {
            System system = new System();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔═══════════════════════════════════════════╗");
                Console.WriteLine("║          AIRBNB GUEST MANAGEMENT          ║");
                Console.WriteLine("║                 SYSTEM                    ║");
                Console.WriteLine("╚═══════════════════════════════════════════╝\n");
                Console.ResetColor();

                Console.WriteLine(new string('─', 45));
                Console.WriteLine("\n1. Reserve Room");
                Console.WriteLine("2. Check-in Guest");
                Console.WriteLine("3. Check-out Guest");
                Console.WriteLine("4. View Room Options");
                Console.WriteLine("5. View Guest");
                Console.WriteLine("6. Sales Report");
                Console.WriteLine("7. Exit\n");
                Console.WriteLine(new string('─', 45));

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("\nSelect an option: ");
                Console.ResetColor();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        system.ReserveRoom();
                        break;

                    case "2":
                        system.CheckIn();
                        break;

                    case "3":
                        system.CheckOut();
                        break;

                    case "4":
                        system.ViewRoomOptions();
                        break;

                    case "5":
                        system.ManageGuestsMenu();
                        break;

                    case "6":
                        system.GenerateSalesReport();
                        break;

                    case "7":
                        exit = true;
                        system.SaveGuestsToFile();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nThank you for using the AirBNB Guest Management System!");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadKey();
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("\nInvalid choice. Please try again.");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ResetColor();
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
