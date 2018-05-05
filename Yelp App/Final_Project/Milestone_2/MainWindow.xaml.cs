using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections;
using System.Text.RegularExpressions;

namespace Milestone_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();       
            InitProgram();
        }

        #region Initializations

        SingletonDB database = SingletonDB.GetInstance;

        // This method does all the functions that have to be done
        // before the program can be used. Mainly populate the
        // States combobox and binding the things needed to
        // be binded to all the data grids
        private void InitProgram()
        {
            AddStates();
            AddColumnToGrid(cityDataGrid, "City", "city", 255);
            AddColumnToGrid(zipcodeGrid, "Zipcodes", "zipcode", 255);
            AddColumnToGrid(searchResultsDataGrid, "Business Name", "bname", 150);
            AddColumnToGrid(searchResultsDataGrid, "Address", "address", 255);
            AddColumnToGrid(searchResultsDataGrid, "Tips", "tips", 100);
            AddColumnToGrid(searchResultsDataGrid, "Checkins", "checkins", 100);
            AddColumnToGrid(categoriesDataGrid, "Categories", "name", 255);
            AddColumnToGrid(addRemoveDataGrid, "Categories", "name", 255);
            AddColumnToGrid(userIDGrid, "User IDs", "uid", 500);
            AddColumnToGrid(friendsDataGrid, "Name", "name", 100);
            AddColumnToGrid(friendsDataGrid, "Rating", "stars", 75);
            AddColumnToGrid(friendsDataGrid, "User ID", "uid", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Name", "name", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Business", "bname", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Tip", "tip", 200);
            AddColumnToGrid(tipsByFriendsGrid, "Date", "date", 200);
            AddColumnToGrid(Bus_CategoriesDataGrid, "Categories", "name", 262);
            AddColumnToGrid(categoriesDataGrid_Account, "Categories", "name", 255);
            AddColumnToGrid(addRemoveDataGrid_Account, "Categories", "name", 350);

        }

        // Runs a query which returns all the distinct states
        // in our database and populates the stateComboBox
        // in the business tab
        public void AddStates()
        {
            string query = "SELECT DISTINCT state FROM yelp_business ORDER BY STATE;";
            var data = database.RunQuery(query);

            PopulateComboBox(data, stateComboBox);
        }

        // AddColumnToGrid adds a column to specified DataGrid, it has 4 parameters:
        // 1. a DataGrid
        // 2. a title for that column
        // 3, and a variable to bind to
        // 4, width of the column
        public void AddColumnToGrid(DataGrid myGrid, string header, string binding, int width)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = header;
            col1.Binding = new Binding(binding);
            col1.Width = width;
            myGrid.Columns.Add(col1);
        }

        #endregion

        #region User Tab

        // Button click event for the search button.
        // Clears all grids depending on the search result
        // of the search button and then Populates the userIDGrid
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            tipsByFriendsGrid.Items.Clear();
            friendsDataGrid.Items.Clear();
            userIDGrid.Items.Clear();
            FindUserIDs(searchTextbox.Text);
        }

        // FindUserIDs opens a connection in the PGadmin DB.
        // It then runs a querry that returns all the user IDs whos names
        // are the same as the name that was entered in the searchTextbox
        private void FindUserIDs(string name)
        {
            // Replacing ' with '' to prevent issues with sql.
            // Also replaces ; with a ;-- to prevent sql injection
            name = name.Replace("'", "''").Replace(";", ";--");
            string query = "SELECT uid FROM yelp_user WHERE name = '" + name + "'";
            var data = database.RunQuery(query);
            PopulateDataGrid(userIDGrid, data, typeof(User));
        }

        // This method is the SelectionChanged event for userIDGrid DataGrid
        // that checks if an item from userIDGrid has been selected, if it has, it calls the PopulateUser method
        // passing the ID of the user as a arguement
        private void userIDGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            friendsDataGrid.Items.Clear();
            tipsByFriendsGrid.Items.Clear();
            friendsTextbox.Text = "Friends : 0";
            tipsByFriendsTextbox.Text = "Friends : 0";

            if (userIDGrid.SelectedItem != null)
            {
                object item = userIDGrid.SelectedItem;
                string ID = ((userIDGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();

                PopulateUserInformation(ID);
            }
        }

        // PopulateUser takes in a user's ID and runs a querry in PGadmin
        // that returns all the information of that user. It then fills all the User information textboxes
        // with the information retrieved from the DB
        void PopulateUserInformation(string ID)
        {
            string query = "SELECT * FROM yelp_user WHERE uid = '" + ID + "'";
            var data = database.RunQuery(query);

            nameTextbox.Text = data["name"][0].ToString();
            starsTextbox.Text = data["stars"][0].ToString();
            fansTextbox.Text = data["fans"][0].ToString();
            funnyTextbox.Text = data["funnyvotes"][0].ToString();
            coolTextbox.Text = data["coolvotes"][0].ToString();
            usefulTextbox.Text = data["usefulvotes"][0].ToString();
            yelpingSinceTextbox.Text = data["yelpingsince"][0].ToString();

            string[] friendsList = data["friends"][0];
            PopulateFriends(friendsList);
        }

        // PopulateFriends populates the friends list of the current selected 
        // user, if they don't have an friends on their account the friends list will remain blank.    
        //
        // It also populates the tipsByFriendsDataGrid ordering it by the most current date
        private void PopulateFriends(string[] friendsList)
        {
            // Only populates the friends date grid
            // if the current user has 1 or more friends
            if (friendsList.Length > 0)
            {
                List<User> friends = new List<User>();
                string query = "SELECT name, stars, uid FROM yelp_user WHERE ";

                bool isFirstIteration = true;
                foreach (string s in friendsList)
                {
                    if (isFirstIteration)
                    {
                        query += "uid = '" + s + "'";
                        isFirstIteration = false;
                    }
                    else
                    {
                        query += " OR uid = '" + s + "'";
                    }
                }

                query += "ORDER BY name";

                var data = database.RunQuery(query);
                PopulateDataGrid(friendsDataGrid, data, typeof(User));
                PopulateTipsByFriends(friendsList);
                friendsTextbox.Text = String.Format("Friends : {0}", friendsList.Length);
            }
            else
            {
                friendsTextbox.Text = "Friends : 0";
            }
        }

        // This method runs the query to get the results that will
        // populate the tipsByFriendsDataGrid. It does
        // This by building a big query string of users for each
        // Each friend in the select users friends list.
        // It runs the query and returns all the tips given by
        // friends by the most recent date.
        private void PopulateTipsByFriends(string[] friendsList)
        {
            string query = "";

            // Loop to build the query
            for (int i = 0; i < friendsList.Length; i++)
            {
                if (i == 0)
                {
                    query = "SELECT u.name, b.bname, t.tip, t.date " +
                            "FROM yelp_business b, yelp_tip t, yelp_user u " +
                            "WHERE b.bid = t.bid AND t.uid = u.uid " +
                            "AND (u.uid = '" + friendsList[i] + "'";
                }
                else
                {
                    query += " OR u.uid = '" + friendsList[i] + "'";
                }
            }

            // Adding a close parenthesis and ORDER BY clause
            // to the end of the query string
            query += ") ORDER BY date desc;";

            // Running query populating the dataGrid
            var data = database.RunQuery(query);
            PopulateDataGrid(tipsByFriendsGrid, data, typeof(Tip));

            // Updating the tipsByFriends counter
            if (tipsByFriendsGrid.Items.Count > 0)
            {
                tipsByFriendsTextbox.Text = String.Format("Latest Tips by Friends : {0}", tipsByFriendsGrid.Items.Count);
            }
            else
            {
                tipsByFriendsTextbox.Text = "Latest Tips by Friends : 0";
            }
        }

        // Event for the Remove Friend button click.
        // Function that runs a query to remove a friend from a user's friend list.
        // User's friend must be selected in order to delete that friend
        private void removeFriendButton_Click(object sender, RoutedEventArgs e)
        {
            if (friendsDataGrid.SelectedIndex > -1)
            {
                User friend = (User)friendsDataGrid.SelectedItem;
                User user = (User)userIDGrid.SelectedItem;
                string uid = friend.uid;
                string usersUID = user.uid;

                string query = "UPDATE yelp_user " +
                               "SET friends = sometable.array_remove " +
                               "FROM " +
                               "(SELECT array_remove(friends, ARRAY['" + uid + "']::TEXT[]), " +
                                   "uid, friends FROM yelp_user WHERE uid = '" + usersUID + "') sometable " +
                               "WHERE " +
                               "yelp_user.uid = sometable.uid;";

                database.RunUpdateQuery(query);

                // After friend gets deleted, the friends
                // and tips by friends data grid get repopulated
                if (userIDGrid.SelectedIndex > -1)
                {
                    friendsDataGrid.Items.Clear();
                    tipsByFriendsGrid.Items.Clear();
                    friendsTextbox.Text = "Friends : 0";

                    string ID = GetCellContentName(userIDGrid);
                    PopulateUserInformation(ID);
                }
            }
        }


        #endregion

        #region Business Tab


        // This method only runs when a state has been selected in the stateComboBox.
        // The purpose is to run a query in the DB and return all distinct cities
        // inside the chosen state.
        private void stateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cityDataGrid.Items.Clear();
            zipcodeGrid.Items.Clear();
            searchResultsDataGrid.Items.Clear();
            categoriesDataGrid.Items.Clear();
            addRemoveDataGrid.Items.Clear();
            BusinessNameTextBox.Clear();

            string query = "SELECT DISTINCT city " +
                           "FROM yelp_business " +
                           "WHERE state = '" + stateComboBox.SelectedItem.ToString() + "' " +
                           "ORDER BY city;";

            var data = database.RunQuery(query);

            PopulateDataGrid(cityDataGrid, data, typeof(Business));
        }

        // This method is the SelectionChanged event for the cityDataGrid.
        // It first clears all other grids that depend on the cityDateGrid (i.e zipcode, search results and categories)
        // and then it runs a query which returns all distinct zipcodes in this city and state
        private void cityDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cityDataGrid.SelectedIndex > -1)
            {
                addRemoveDataGrid.Items.Clear();
                categoriesDataGrid.Items.Clear();
                searchResultsDataGrid.Items.Clear();
                zipcodeGrid.Items.Clear();
                BusinessNameTextBox.Clear();

                string state = stateComboBox.SelectedItem.ToString();
                string city = GetCellContentName(cityDataGrid);

                string query = "SELECT DISTINCT zipcode " +
                               "FROM yelp_business " +
                               "WHERE state = '" + state + "' " +
                               "AND city = '" + city + "' " +
                               "ORDER BY zipcode;";

                var data = database.RunQuery(query);
                PopulateDataGrid(zipcodeGrid, data, typeof(Business));
            }
        }

        // This method is only ran when the user selects a zipcode from the 
        // the zipcodeDataGrid. The purpose is to fill up the searchResultsDataGrid
        // and the categoriesDataGrid using the state, city and zipcode selected
        private void zipcodeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (zipcodeGrid.SelectedIndex > -1)
            {
                searchResultsDataGrid.Items.Clear();
                categoriesDataGrid.Items.Clear();
                BusinessNameTextBox.Clear();
                addRemoveDataGrid.Items.Clear();
                dayOfWeekCombo.Items.Clear();
                FromCombo.Items.Clear();
                toCombo.Items.Clear();

                string state = stateComboBox.SelectedItem.ToString();
                string city = GetCellContentName(cityDataGrid);
                string zipcode = zipcode = GetCellContentName(zipcodeGrid);

                //Dont forget to add hoursopen
                string query = "SELECT bname, address, tips, checkins, bid, latitude, longitude " +
                               "FROM yelp_business " +
                               "WHERE state = '" + state + "' " +
                               "AND city = '" + city + "' " +
                               "AND zipcode = '" + zipcode + "' " +
                               "ORDER BY bname;";

                var data = database.RunQuery(query);
                PopulateDataGrid(searchResultsDataGrid, data, typeof(Business));

                PopulateCategories(state, city, zipcode);
                PopulateTimeFilter();
            }
        }

        // Updates the businessNameTextbox to the currently selected business from searchResultsDataGrid
        private void searchResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BusinessNameTextBox.Clear();

            if (searchResultsDataGrid.SelectedIndex > -1)
            {
                Business business = (Business)searchResultsDataGrid.SelectedItem;
                BusinessNameTextBox.Text = business.bname;
                BusinessNameTextBox.FontSize = 15;
            }
        }

        // This method is the button click event for the Checkin button.
        // It's purpose is to run update queries to the yelp_checkins
        // and yelp_business tables to appropriatly add 1 to the checkin amount
        private void CheckinButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchResultsDataGrid.SelectedIndex > -1)
            {
                string hour = GetTime();
                int i = 0;

                // These if states figure out which array index needs to be
                // called in the update statement for the yelp_checkins table
                // 1 - 06:00 <= checkinTime < 12:00
                // 2 - 12:00 <= checkinTime < 17:00
                // 3 - 17:00 <= checkinTime < 23:00
                // 4 - 23:00 <= checkinTime < 06:00
                if ((hour.CompareTo("06:00") > 0 || hour.CompareTo("06:00") == 0) &&
                    hour.CompareTo("12:00") < 0)
                {
                    i = 1;
                }
                else if ((hour.CompareTo("12:00") > 0 || hour.CompareTo("12:00") == 0) &&
                        hour.CompareTo("17:00") < 0)
                {
                    i = 2;
                }
                else if ((hour.CompareTo("17:00") > 0 || hour.CompareTo("17:00") == 0) &&
                        hour.CompareTo("23:00") < 0)
                {
                    i = 3;
                }
                else if ((hour.CompareTo("23:00") > 0 || hour.CompareTo("23:00") == 0) ||
                        hour.CompareTo("6:00") < 0)
                {
                    i = 4;
                }

                Business business = searchResultsDataGrid.SelectedItem as Business;
                string query =
                    String.Format("UPDATE yelp_checkins SET {0}[{1}] = {0}[{1}] + 1 WHERE bid = '{2}'", GetDay(), i, business.bid);

                string query2 =
                    String.Format("UPDATE yelp_business SET checkins = checkins + 1 WHERE bid = '{0}'", business.bid);

                database.RunUpdateQuery(query);
                database.RunUpdateQuery(query2);
                TimeFilterAndCategoryFilter();
            }
        }

        // AddTip button click
        // When a user is selected and a business is selected then the user can
        // Enter some text into the tip text box and this tip will be added
        // into the tips table and tip chart for that specific business.
        private void AddTipButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchResultsDataGrid.SelectedIndex > -1 && userIDGrid.SelectedIndex > -1)
            {
                if (addTipTextbox.Text.Length < 1)
                {
                    return;
                }
                string tip = addTipTextbox.Text;
                tip = tip.Replace("'", "''").Replace(";", ";--");
                Business business = (Business)searchResultsDataGrid.SelectedItem;
                User user = (User)userIDGrid.SelectedItem;

                string uid = user.uid;
                string bid = business.bid;
                string query = string.Format("INSERT INTO yelp_tip(bid, uid, tip, date)" +
                    " values" +
                    "('{0}', '{1}', '{2}', current_date)", bid, uid, tip);
                string query2 = string.Format("Update yelp_business" +
                    " set tips = tips +1" +
                    " where bid = '{0}'", bid);
                addTipTextbox.Clear();
                database.RunUpdateQuery(query2);
                database.RunUpdateQuery(query);
                TimeFilterAndCategoryFilter();
            }
        }

        // This method returns ALL distinct categories
        // from the business table and 
        // populates a given dataGrid
        private void SetDistinctCategories(DataGrid dataGrid)
        {
            string query = "SELECT DISTINCT name FROM" +
                           "( " +
                           "SELECT UNNEST(categories) as name " +
                                 "FROM yelp_business " +
                            ") mytable " +
                            "ORDER BY name;";

            var data = database.RunQuery(query);
            PopulateDataGrid(dataGrid, data, typeof(Category));
        }

        #endregion

        #region Categories

        // This method populates the categories dataGrid
        // based on which state, city and zipcode are selected.
        // It does this by running a query that returns all distinct
        // categories from that area and sorts them alphabetically
        private void PopulateCategories(string state, string city, string zipcode)
        {
            string query = "SELECT DISTINCT unnest(categories) as name " +
                           "FROM yelp_business " +
                           "WHERE state = '" + state + "' " +
                           "AND city = '" + city + "' " +
                           "AND zipcode = '" + zipcode + "' " +
                           "ORDER BY name";

            var data = database.RunQuery(query);
            PopulateDataGrid(categoriesDataGrid, data, typeof(Category));
        }

        // Button click  event for the "Add" button that calls a method
        // to Add the category from the categoriesDataGrid
        // to the addRemoveDataGrid. It then sorts the grid alphabetically
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesDataGrid.SelectedIndex > -1)
            {
                ChangeAddRemoveDataGrid(addRemoveDataGrid, categoriesDataGrid);
            }
        }

        // Button click  event for the "Add" button that calls a method
        // to Add the category from the categoriesDataGrid
        // to the addRemoveDataGrid. It then sorts the grid alphabetically
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (addRemoveDataGrid.SelectedIndex > -1)
            {
                ChangeAddRemoveDataGrid(categoriesDataGrid, addRemoveDataGrid);
            }
        }

        // This method removes a category from one dataGrid, and adds it to the next
        // dataGrid depending on which 2 dataGrids are passed. And then
        // reruns the query for the searchResultsDataGrid with the added or
        // removed categories.
        private void ChangeAddRemoveDataGrid(DataGrid addTo, DataGrid removeFrom)
        {
            var itemToMove = removeFrom.SelectedItem;
            removeFrom.Items.RemoveAt(removeFrom.SelectedIndex);
            addTo.Items.Add(itemToMove as Category);
            SortCategoriesGrid(addTo);

            if (addTo == categoriesDataGrid || addTo == addRemoveDataGrid)
            {
                string state = stateComboBox.SelectedItem.ToString();
                string city = GetCellContentName(cityDataGrid);
                string zipcode = GetCellContentName(zipcodeGrid);
                BusinessNameTextBox.Clear();
                TimeFilterAndCategoryFilter();
            }
        }

        #endregion

        #region Time Filter

        // Method that populates the dayOfWeekCombo box.
        // Only gets called when a zipcode has been selected.
        public void PopulateTimeFilter()
        {
            dayOfWeekCombo.Items.Add("Sunday");
            dayOfWeekCombo.Items.Add("Monday");
            dayOfWeekCombo.Items.Add("Tuesday");
            dayOfWeekCombo.Items.Add("Wednesday");
            dayOfWeekCombo.Items.Add("Thursday");
            dayOfWeekCombo.Items.Add("Friday");
            dayOfWeekCombo.Items.Add("Saturday");
        }


        // Event for when a day of the week has been selected in the dayOfWeek
        private void dayOfWeekCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toCombo.Items.Clear();
            FromCombo.Items.Clear();

            // Populating the fromCombo box with 24 hours
            // in the format  00:00....23:00
            if (dayOfWeekCombo.SelectedIndex > -1)
            {
                for (int i = 0; i < 24; i++)
                {
                    if (i < 10)
                    {
                        FromCombo.Items.Add(String.Format("0{0}:00", i.ToString()));
                    }
                    else
                    {
                        FromCombo.Items.Add(String.Format("{0}:00", i.ToString()));
                    }
                }
            }
        }

        // This method only gets called once a time has been chosen in the fromCombo.
        // It then adds all the hours in the toCombo. Doing it this way prevents
        // a user to select a start time earlier than an end time
        private void FromCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toCombo.Items.Clear();
            if (FromCombo.SelectedIndex > -1)
            {
                for (int i = FromCombo.SelectedIndex; i < 24; i++)
                {
                    if (i < 10)
                    {
                        toCombo.Items.Add(String.Format("0{0}:00", i.ToString()));
                    }
                    else
                    {
                        toCombo.Items.Add(String.Format("{0}:00", i.ToString()));
                    }
                }
            }
        }

        // Button click for the Apply Button in the time filter section.
        // It's purpose is to call a method that repopulates the searchResultsDataGrid
        // with the new added time filter
        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (dayOfWeekCombo.SelectedIndex > -1 &&
                FromCombo.SelectedIndex > -1 && toCombo.SelectedIndex > -1)
            {
                TimeFilterAndCategoryFilter();
            }
        }

        #endregion

        #region Charts

        // When a business is selected in the business search results, we can view the number of checkins that
        // the business has for every day of the week by opening a new window from our ChartWindow Class.
        private void showCheckinsButton_Click(object sender, RoutedEventArgs e)
        {
            ChartWindow chartWindow = new ChartWindow();

            if (searchResultsDataGrid.SelectedIndex > -1)
            {
                Business business = (Business)searchResultsDataGrid.SelectedItem;

                chartWindow.CheckinChart(business.bid);
                chartWindow.SetTitle(business.bname);
                chartWindow.Title = "Business Checkins";

                chartWindow.Show();
                chartWindow.Topmost = true;
            }
        }

        // Function that is used to display a chart for all the tips written about a certain business
        // by opening a new window using the tipsWindow class
        private void showTipsButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchResultsDataGrid.SelectedIndex > -1)
            {
                Business business = (Business)searchResultsDataGrid.SelectedItem;
                TipsWindow tipsWindow = new TipsWindow();

                string query = String.Format("select u.name, t.date, t.tip, t.likes from yelp_user u, " +
                    "yelp_tip t, yelp_business b where b.bid = t.bid and u.uid = " +
                    "t.uid and b.bid = '{0}'order by date desc", business.bid);

                var data = database.RunQuery(query);
                PopulateDataGrid(tipsWindow.tipsForBusinessDataGrid, data, typeof(Tip));
                tipsWindow.SetTitle(String.Format("Tips for {0}", business.bname));
                tipsWindow.Show();
            }
        }

        // When a certain city is selected, we can view the amount of business
        // per zipcode with this function
        private void businessPerZipCodeButton_Click(object sender, RoutedEventArgs e)
        {
            ChartWindow chartWindow = new ChartWindow();

            if (cityDataGrid.SelectedIndex > -1)
            {
                Business city = (Business)cityDataGrid.SelectedItem;

                chartWindow.ZipChart(city.city);
                chartWindow.SetTitle(String.Format("Number of Businesses Per Zipcode in {0}", city.city));
                chartWindow.Title = "Businesses Per Zipcode";

                chartWindow.Show();
                chartWindow.Topmost = true;
            }
        }

        #endregion

        #region Map
        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchResultsDataGrid.Items.IsEmpty)
            {
                return;
            }
            MapWindow myMap = new MapWindow();

            if (searchResultsDataGrid.SelectedIndex > -1)
            {

                // DO method for 1 bidness.  // Zoom level 15-16
                Business business = (Business)searchResultsDataGrid.SelectedItem;
                myMap.AddPushPins(business.latitude, business.longitude, 16);

            }

            else if (!searchResultsDataGrid.Items.IsEmpty)
            {

                // Zoom level 9-11?
                foreach (Business business in searchResultsDataGrid.Items)
                {
                    myMap.AddPushPins(business.latitude, business.longitude, 10);
                }

            }
            myMap.Show();
        }
        #endregion

        #region LoggedInTab

        // Displays tips for the business that is loaded in the logged in page
        private void TipsForThisBusinessButton_Click(object sender, RoutedEventArgs e)
        {
            if(BIDTextBox.Text != string.Empty)
            {
                TipsWindow tipsWindow = new TipsWindow();
                string query = String.Format("select u.name, t.date, t.tip, t.likes from yelp_user u, " +
               "yelp_tip t, yelp_business b where b.bid = t.bid and u.uid = " +
               "t.uid and b.bid = '{0}'order by date desc", BIDTextBox.Text);


                var data = database.RunQuery(query);
                PopulateDataGrid(tipsWindow.tipsForBusinessDataGrid, data, typeof(Tip));
                tipsWindow.SetTitle(String.Format("Tips for {0}", BusinessNameTextBlock.Text));
                tipsWindow.Show();
            }
        }


        // From the Business logged in page, a user can view their business location on a map
        private void DisplayOnMapButton_Click(object sender, RoutedEventArgs e)
        {
            if (BIDTextBox.Text != string.Empty)
            {
                MapWindow myMap = new MapWindow();
                string query = String.Format("select latitude, longitude from yelp_business" +
                    " where bid = '{0}'", BIDTextBox.Text);

                var data = database.RunQuery(query);
                
                myMap.AddPushPins(data["latitude"][0], data["longitude"][0], 16);
                myMap.Show();
            }
        }


        // Not working yet
        // Once a user has logged into their account, this button takes them to another page
        // That allows them to edit their business information.
        private void EditInfoButton_Click(object sender, RoutedEventArgs e)
        {
            EditBusinessInformationButton.Content = "Coming Soon";
        }


        // Function to populate the logged in page for a business
        // Takes in a business id string as a parameter and uses that to run a query to 
        // Find out everything about that specific business
        private void PopulateBusinessLoginPage(string bid)
        {
            // Populate Categories
            ClearBusinessLoginPage();
            string catQuery = string.Format("select unnest(categories) as name from yelp_business where bid = '{0}'", bid);
            // Query is run to populate the categories datagrid
            var catData = database.RunQuery(catQuery);
            PopulateDataGrid(Bus_CategoriesDataGrid, catData, typeof(Category));

            // Query is run to receive all the relevant information about a specific business
            string businessQuery = string.Format("Select * from yelp_business where bid = '{0}'", bid);
            var businessData = database.RunQuery(businessQuery);

            // Business hours stored in separate hours array.
            string[,] hours = new string[7, 3];
            hours = businessData["hours"][0];

            // If business hours are {-1,-1} (our syntax for closed)
            // Then we replace that string with closed
            for(int i = 0; i < 7; i++)
            {
                if(hours[i,1] == "-1")
                {
                    hours[i, 1] = "Closed";
                    hours[i, 2] = "Closed";
                }
            }
            
            // Fill in every text box with relevant information
            BusinessAddressTextBox.Document.Blocks.Add(new Paragraph(new Run(businessData["address"][0].ToString())));
            BusinessCityTextBox.Document.Blocks.Add(new Paragraph(new Run(businessData["city"][0])));
            BusinessNameTextBlock.Text = businessData["bname"][0];
            BusinessStateTextBox.Document.Blocks.Add(new Paragraph(new Run(businessData["state"][0])));
            BusinessZipTextBox.Document.Blocks.Add(new Paragraph(new Run(businessData["zipcode"][0])));
            BIDTextBox.Text = businessData["bid"][0];
            BusinessTipsTextBox.Text = businessData["tips"][0].ToString();
            BusinessStarsTextBox.Text = businessData["stars"][0].ToString();
            BusinessReviewCountTextBox.Text = businessData["reviewcount"][0].ToString();
            BusinessCheckinsTextBox.Text = businessData["checkins"][0].ToString();
            // Fill in hours text boxes
            sunday_Open.Text = hours[0,1];
            sunday_Close.Text = hours[0, 2];
            monday_Open.Text = hours[1, 1];
            monday_Close.Text = hours[1, 2];
            tuesday_Open.Text = hours[2, 1];
            tuesday_Close.Text = hours[2, 2];
            wednesday_Open.Text = hours[3, 1];
            wednesday_Close.Text = hours[3, 2];
            thursday_Open.Text = hours[4, 1];
            thursday_Close.Text = hours[4, 2];
            friday_Open.Text = hours[5, 1];
            friday_Close.Text = hours[5, 2];
            saturday_Open.Text = hours[6, 1];
            saturday_Close.Text = hours[6, 2];
        }

        // When the logout button is clicked, all information in login window is cleared
        // Logged in tab is collapsed, and the user is sent to
        // the tab where they enter their credentials
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            createAccountTab.Visibility = Visibility.Collapsed;
            ClearCreateAccountTab();
            loginPassTextbox.Clear();
            forgotPasswordButton.Content = "Forgot Password?";
            ClearBusinessLoginPage();
            loginTab.Visibility = Visibility.Visible;
            businessOwnerTab.Visibility = Visibility.Collapsed;
            tabControl.SelectedIndex = 2;
        }

        // Completely clears out everything inside the business info page
        private void ClearBusinessLoginPage()
        {
            sunday_Close.Clear();
            sunday_Open.Clear();
            monday_Close.Clear();
            monday_Open.Clear();
            tuesday_Close.Clear();
            tuesday_Open.Clear();
            wednesday_Close.Clear();
            wednesday_Open.Clear();
            thursday_Close.Clear();
            thursday_Open.Clear();
            friday_Close.Clear();
            friday_Open.Clear();
            saturday_Close.Clear();
            saturday_Open.Clear();
            BusinessAddressTextBox.Document.Blocks.Clear();
            BusinessCityTextBox.Document.Blocks.Clear();
            BusinessNameTextBlock.Text = string.Empty;
            BusinessStateTextBox.Document.Blocks.Clear();
            BusinessZipTextBox.Document.Blocks.Clear();
            BIDTextBox.Clear();
            BusinessTipsTextBox.Clear();
            BusinessStarsTextBox.Clear();
            BusinessReviewCountTextBox.Clear();
            BusinessCheckinsTextBox.Clear();
            Bus_CategoriesDataGrid.Items.Clear();
        }

        // Event that displays a checkin chart for a business
        // Called from the Logged in tab for businesses
        private void BusinessCheckinsButton_Click(object sender, RoutedEventArgs e)
        {
            ChartWindow chartWindow = new ChartWindow();
            if (BIDTextBox.Text != string.Empty)
            {

                    chartWindow.CheckinChart(BIDTextBox.Text);
                    chartWindow.SetTitle(BusinessNameTextBlock.Text);
                    chartWindow.Title = "Business Checkins";

                    chartWindow.Show();
                    chartWindow.Topmost = true;
            }
        }

        // Verifies login credentials
        private string VerifyLogin()
        {
            HashKey hash = new HashKey();
            string username = usernameTextbox.Text;
            string password = hash.GetPasswordHash(loginPassTextbox.Password).Substring(0, 30);
            string query = string.Format("select * from yelp_account" +
                " where username = '{0}' AND password = '{1}'", username, password);
            if (database.IsEmpty(query))
            {
                return string.Empty;
            }
            var data = database.RunQuery(query);
            return data["bid"][0].ToString();
        }
        #endregion

        #region Create Account

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            loginTab.Visibility = Visibility.Collapsed;
            businessOwnerTab.Visibility = Visibility.Collapsed;
            createAccountTab.Visibility = Visibility.Visible;
            SetDistinctCategories(categoriesDataGrid_Account);
            tabControl.SelectedIndex = 4;
        }

        // This method verifies if the credentials for the new account are valid.
        // Checks if the username is unqiue, and 
        // also checks if the passwords match.
        private void verifyButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyAccountInformation();
        }

        private bool VerifyAccountInformation()
        {
            if (createUsernameTextbox.Text.Length > 0 &&
                    passwordTextbox.Password.Length > 0 &&
                    confirmPasswordTextbox.Password.Length > 0)
            {
                string query = String.Format("SELECT * FROM yelp_account " +
                    "WHERE username = '{0}';", RemoveSQLIssues(createUsernameTextbox.Text));

                // Check if username already exists
                if (!database.IsEmpty(query))
                {
                    verifyLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                    verifyLabel.Content = "Username already exists!";
                    return false;
                }

                // Checks if passwords match
                if (passwordTextbox.Password.CompareTo(confirmPasswordTextbox.Password) != 0)
                {
                    verifyLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                    verifyLabel.Content = "Passwords don't match!";
                    return false;
                }

                verifyLabel.Foreground = new SolidColorBrush(Colors.ForestGreen);
                verifyLabel.Content = "Credentials are good!";
                return true;
            }
            else
            {
                verifyLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                verifyLabel.Content = "Your fields are empty!";
                return false;
            }
        }

        // This method is the event for the "Add" button click on the
        // Create Account tab. It's purpose is to move a category
        // from the data grid on the left, to the data grid on the right
        private void addButton_Account_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesDataGrid_Account.SelectedIndex > -1)
            {
                ChangeAddRemoveDataGrid(addRemoveDataGrid_Account, categoriesDataGrid_Account);
                SortCategoriesGrid(addRemoveDataGrid_Account);
            }
        }

        // This method is the event for the "Remove" button click on the
        // Create Account tab. It's purpose is to move a category
        // from the data grid on the right, to the data grid on the left
        private void removeButton_Account_Click(object sender, RoutedEventArgs e)
        {
            if (addRemoveDataGrid_Account.SelectedIndex > -1)
            {
                ChangeAddRemoveDataGrid(categoriesDataGrid_Account, addRemoveDataGrid_Account);
                SortCategoriesGrid(categoriesDataGrid_Account);
            }
        }

        // Returns true if the bnameTextbox
        // is not empty
        private bool VerifyBname(string text)
        {
            if (text.Length > 0)
            {
                return true;
            }
            return false;
        }

        // Returns true if the stateTextbox in the
        // accountTab is both length 2 and contains
        // ONLY letters
        private bool VerifyState(string text)
        {
            if (text.Length == 2 && Regex.IsMatch(text, @"^[a-zA-Z]+$"))
            {
                return true;
            }
            return false;
        }

        // Check to see if the user entered a valid state
        // by seeing if the length is greater than 1 and
        // the name only contains letters
        private bool VerifyCity(string text)
        {
            if (text.Length > 0 && Regex.IsMatch(text, @"^[a-zA-Z\s\-\']+$"))
            {
                return true;
            }
            return false;
        }

        // Check to verify that the user entered 
        // a valid zipcode. The zipcode has to be a
        // 5 digit number
        private bool VerifyZipcode(string text)
        {
            if (text.Length == 5 && Regex.IsMatch(text, @"^\d+$"))
            {
                return true;
            }
            return false;
        }

        // Check to verify if the user entered a valid address.
        // Needs to be 1 or more character, also needs to
        // contain the state, city and zipcode in it
        private bool VerifyAddress(string text)
        {
            if (text.Length > 0)
            {
                return true;
            }
            return false;
        }

        // Check to verify that the user entered valid coordinates.
        // They must be doubles between 0 and 180
        private bool VerifyCoordinates(string latitude, string longitude)
        {
            var latIsNumeric = double.TryParse(latitude, out double i);
            var longIsNumeric = double.TryParse(longitude, out double j);

            if (latIsNumeric && longIsNumeric && i <= 180 && i >= -180 && j <= 180 && j >= -180)
            {
                return true;
            }
            return false;
        }

        // This method checks if the user
        // entered the times in the correct format
        // in the Create Account tab.
        // If breaks the strings into parts
        // and basically checks if it's between 00:00
        // and 23:59
        private bool VerifyOpenTimes(string text)
        {
            if(text == "-1")
            {
                return true;
            }
            if (text.Length == 5)
            {
                string firstHalf = text.Substring(0, 2);
                string secondHalf = text.Substring(3, 2);
                char middle = text[2];

                if (Regex.IsMatch(firstHalf, @"^\d+$") && Regex.IsMatch(secondHalf, @"^\d+$"))
                {
                    int first = Convert.ToInt32(firstHalf);
                    int second = Convert.ToInt32(secondHalf);

                    if (first >= 0 && first <= 23
                        && second >= 0 && first <= 59
                        && middle == ':')
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Event for the Create account button click
        private void createAccountButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyAccountCreation();
        }

        //
        private bool VerifyAccountCreation()
        {
            if (VerifyAccountInformation())
            {
                List<bool> verifications = new List<bool>();

                verifications.Add(VerifyBname(bnameTextbox.Text));
                verifications.Add(VerifyState(stateTextbox.Text));
                verifications.Add(VerifyZipcode(zipcodeTextbox.Text));
                verifications.Add(VerifyAddress(fullAddressTextbox.Text));
                verifications.Add(VerifyCoordinates(latitudeTextbox.Text, longitudeTextbox.Text));
                verifications.Add(VerifyOpenTimes(sundayOpen.Text));
                verifications.Add(VerifyOpenTimes(sundayClose.Text));
                verifications.Add(VerifyOpenTimes(mondayOpen.Text));
                verifications.Add(VerifyOpenTimes(mondayClose.Text));
                verifications.Add(VerifyOpenTimes(tuesdayOpen.Text));
                verifications.Add(VerifyOpenTimes(tuesdayClose.Text));
                verifications.Add(VerifyOpenTimes(wednesdayOpen.Text));
                verifications.Add(VerifyOpenTimes(wednesdayClose.Text));
                verifications.Add(VerifyOpenTimes(thursdayOpen.Text));
                verifications.Add(VerifyOpenTimes(thursdayClose.Text));
                verifications.Add(VerifyOpenTimes(fridayOpen.Text));
                verifications.Add(VerifyOpenTimes(fridayClose.Text));
                verifications.Add(VerifyOpenTimes(saturdayOpen.Text));
                verifications.Add(VerifyOpenTimes(saturdayClose.Text));

                foreach (bool verified in verifications)
                {
                    if (!verified)
                    {
                        AccountCreationFailed();
                        return false;
                    }
                }
                CreateNewBusiness();
                return true;
            }
            AccountCreationFailed();
            return false;
        }

        private void AccountCreationFailed()
        {
            AccountConfirm fail = new AccountConfirm();
            fail.ConfirmFailed();
            fail.Show();
        }

        private void AccountCreationSucceeded()
        {
            createAccountTab.Visibility = Visibility.Collapsed;
            ClearCreateAccountTab();
            forgotPasswordButton.Content = "Forgot Password?";
            loginTab.Visibility = Visibility.Visible;
            tabControl.SelectedIndex = 2;
        }

        // Method that is run only when all the
        // create account requirements have
        // been successfully filled.
        // It gets all the information from
        // the textbox and creates a new business
        // in the yelp_business table and 
        // a new account in yelp_account
        private void CreateNewBusiness()
        {
            HashKey hk = new HashKey();

            string username = RemoveSQLIssues(createUsernameTextbox.Text);
            string password = RemoveSQLIssues(hk.GetPasswordHash(passwordTextbox.Password)).Substring(0, 30);
            string bname = RemoveSQLIssues(bnameTextbox.Text);
            string bid = hk.GetRandomMD5HashValue().Substring(0, 22);
            string state = RemoveSQLIssues(stateTextbox.Text);
            string city = RemoveSQLIssues(cityTextbox.Text);
            string zipcode = RemoveSQLIssues(zipcodeTextbox.Text).Substring(0, 5);
            string address = RemoveSQLIssues(fullAddressTextbox.Text);
            string latitude = latitudeTextbox.Text;
            string longitude = longitudeTextbox.Text;
            string t1 = sundayOpen.Text;
            string t2 = sundayClose.Text;
            string t3 = mondayOpen.Text;
            string t4 = mondayClose.Text;
            string t5 = tuesdayOpen.Text;
            string t6 = tuesdayClose.Text;
            string t7 = wednesdayOpen.Text;
            string t8 = wednesdayClose.Text;
            string t9 = thursdayOpen.Text;
            string t10 = thursdayClose.Text;
            string t11 = fridayOpen.Text;
            string t12 = fridayClose.Text;
            string t13 = saturdayOpen.Text;
            string t14 = saturdayClose.Text;
            string cat = "{}";

            if (addRemoveDataGrid_Account.Items.Count > 0)
            {
                cat = "{";
                foreach (Category c in addRemoveDataGrid_Account.Items)
                {
                    if (cat.Length == 1)
                    {
                        cat += c.name;
                    }
                    else
                    {
                        cat += String.Format(",{0}", c.name);
                    }
                }
                cat += "}";
            }

            string query = "INSERT INTO yelp_business(bid, bname, address, " +
                           "city, state, latitude, longitude, stars, reviewcount, " +
                           "openstatus, categories, hours, checkins, zipcode, tips) " +
                           "VALUES " +
                                "( " +
                                "'" + bid + "', " +
                                "'" + bname + "', " +
                                "'" + address + "', " +
                                "'" + city + "', " +
                                "'" + state.ToUpper() + "', " +
                                "'" + latitude + "', " +
                                "'" + longitude + "', " +
                                "'" + 0 + "', " +
                                "'" + 0 + "', " +
                                "'true', " +
                                "'" + cat + "', " +
                                "'{{Sunday," + t1 + "," + t2 + "}," +
                                "{Monday," + t3 + "," + t4 + "}," +
                                "{Tuesday," + t5 + "," + t6 + "}," +
                                "{Wednesday," + t7 + "," + t8 + "}," +
                                "{Thursday," + t9 + "," + t10 + "}," +
                                "{Friday," + t11 + "," + t12 + "}," +
                                "{Saturday," + t13 + "," + t14 + "}" +
                                "}'," +
                                "'0'," +
                                "'" + zipcode + "'," +
                                "'0');";

            database.RunUpdateQuery(query);

            string query2 = String.Format("INSERT into yelp_account(bid, username, password)" +
                            "VALUES('{0}','{1}','{2}');", bid, username, password);

            database.RunUpdateQuery(query2);
            AccountCreationSucceeded();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            createAccountTab.Visibility = Visibility.Collapsed;
            ClearCreateAccountTab();
            forgotPasswordButton.Content = "Forgot Password?";
            loginTab.Visibility = Visibility.Visible;
            tabControl.SelectedIndex = 2;
        }

        // This method clears all WPF objects in the Create Account tab
        private void ClearCreateAccountTab()
        {
            // Clearing Account Information section
            usernameTextbox.Clear();
            passwordTextbox.Clear();
            confirmPasswordTextbox.Clear();
            verifyLabel.Content = "";

            // Clearing hours section
            sundayOpen.Clear();
            sundayClose.Clear();
            mondayOpen.Clear();
            mondayClose.Clear();
            tuesdayOpen.Clear();
            tuesdayClose.Clear();
            wednesdayOpen.Clear();
            wednesdayClose.Clear();
            thursdayOpen.Clear();
            thursdayClose.Clear();
            fridayOpen.Clear();
            fridayClose.Clear();
            saturdayOpen.Clear();
            saturdayClose.Clear();

            // Clearing Business Information Section
            bnameTextbox.Clear();
            stateTextbox.Clear();
            cityTextbox.Clear();
            zipcodeTextbox.Clear();
            fullAddressTextbox.Clear();
            latitudeTextbox.Clear();
            longitudeTextbox.Clear();
            categoriesDataGrid_Account.Items.Clear();
            addRemoveDataGrid_Account.Items.Clear();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string vid = VerifyLogin();
            if(vid != string.Empty)
            {
                usernameTextbox.Clear();
                passwordTextbox.Clear();
                PopulateBusinessLoginPage(vid);
                loginTab.Visibility = Visibility.Collapsed;
                businessOwnerTab.Visibility = Visibility.Visible;
                tabControl.SelectedIndex = 3;

            }
            else
            {
                AccountConfirm window = new AccountConfirm();
                window.LoginFailed();
                loginPassTextbox.Clear();
                usernameTextbox.Clear();
                window.Show();
            }
        }

        private void forgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            forgotPasswordButton.Content = "Too bad...";
        }

        #endregion

        #region Non GUI Methods

        // This method is the Filter method for when there
        // is either a category filter, a time filter, or both
        // at the same time. It runs a query to the database
        // to apply all these filters.
        private void TimeFilterAndCategoryFilter()
        {
            string state = stateComboBox.SelectedItem.ToString();
            string city = GetCellContentName(cityDataGrid);
            string zipcode = GetCellContentName(zipcodeGrid);
            int dayIndex = 0;
            string fromHour;
            string toHour;
            string categories = string.Empty;

            // Adding categories to the query, if there are any
            int i = 0;
            foreach (Category category in addRemoveDataGrid.Items)
            {
                if (i == 0)
                {
                    categories += String.Format("WHERE cat.category in('{0}'", category.name);
                }
                else
                {
                    categories += String.Format(",'{0}'", category.name);
                }
                i++;
            }

            if (i > 0)
            {
                categories += ")";
            }

            string time;

            // Applying the time filter
            // to the query
            if (toCombo.SelectedIndex < 0)
            {
                time = "";
            }
            else
            {
                dayIndex = dayOfWeekCombo.SelectedIndex + 1;
                fromHour = FromCombo.SelectedItem.ToString();
                toHour = toCombo.SelectedItem.ToString();
                time = String.Format(" AND t.opentime <= '{0}' AND t.closetime > '{1}'", fromHour, toHour);
            }

            // Query that is used to filter businesses based on times and categories
            string query = String.Format(
                            "SELECT bname, t.address, tips, b.checkins, b.bid, b.latitude, b.longitude FROM yelp_business b, " +
                            "( " +
                                "SELECT count(cat.category), cat.bid FROM " +
                                "( " +
                                    "SELECT unnest(categories) category, bid, zipcode FROM Yelp_business " +
                                    "WHERE state = '{0}' AND city = '{1}' AND zipcode = '{2}' " +
                                ") cat " +
                                "{3} " +
                                "GROUP BY cat.bid " +
                            ") mytable, " +
                            "( " +
                                "SELECT hours[{4}][1] as day, hours[{4}][2] as opentime, hours[{4}][3] as closetime, bid, address, zipcode " +
                                "FROM yelp_business " +
                                "WHERE state = '{0}' AND city = '{1}' AND zipcode = '{2}' " +
                            ") t " +
                            "WHERE mytable.count > {5} AND b.bid = mytable.bid AND t.bid = b.bid " +
                            "{6} ORDER BY bname",
                            state, city, zipcode, categories, dayIndex, i - 1, time);

            searchResultsDataGrid.Items.Clear();
            var data = database.RunQuery(query);
            PopulateDataGrid(searchResultsDataGrid, data, typeof(Business));
        }

        // Method that populates any combo box given the correct
        // data and comboBox
        public void PopulateComboBox(dynamic data, ComboBox comboBox)
        {
            if (data != null)
            {
                foreach (KeyValuePair<string, dynamic> kvp in data)
                {
                    foreach (var item in data[kvp.Key])
                    {
                        comboBox.Items.Add(item);
                    }
                }
            }
            else
            {
                // Error in case the dictionary data is empty
                throw new ArgumentException("data dictionary is empty!");
            }
        }

        // This method populates any dataGrid assuming the correct
        // date and type have been passed as well. 
        // It first uses the type passed in to create a generic list of that type. E.g List<Business> or List<User> etc..
        // Then it iterates through the data dictionary and sets specific variables 
        // using the key of the dictionary to the values in the dictionary.
        // E.g if a dictionary key is "name" and it's value[0] = "john", then the
        // program tells my obj to set the variable - "name" = "john".
        private void PopulateDataGrid(DataGrid dataGrid, dynamic data, Type type)
        {
            IList objectsToAddToDataGrid = CreateList(type);

            // This outer loop uses the first key value pair
            // to get the length of the list in the value
            foreach (var kvp in data)
            {
                // This loop iterates n amount of times where
                // n is the length of the list (All lists in the dictionary
                // are of same length)
                for (int i = 0; i < data[kvp.Key].Count; i++)
                {
                    // creating an instance of the specified type
                    var tempObject = Activator.CreateInstance(type);

                    // Looping n times where n = dictionary.count
                    // and using the GetProperty().SetValue() functions
                    // to set a specific variable to a certain value
                    foreach (var kvp2 in data)
                    {
                        tempObject.GetType().GetProperty(kvp2.Key).SetValue(tempObject, data[kvp2.Key][i], null);
                    }
                    objectsToAddToDataGrid.Add(tempObject);
                }
                break;
            }

            // Populating the dataGrid with the correct objects
            foreach (var item in objectsToAddToDataGrid)
            {
                dataGrid.Items.Add(item);
            }
        }

        // Creates a list of a specific type needed
        public IList CreateList(Type type)
        {
            Type genericList = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericList);
        }

        // This method returns the name of the content inside the cell
        // of a row in a dataGrid the user selected
        private string GetCellContentName(DataGrid dataGrid)
        {
            object item = dataGrid.SelectedItem;
            return ((dataGrid.SelectedCells[0].Column.GetCellContent(item) as TextBlock).Text).ToString();
        }

        // Gets the current day E.g "Monday, Tuesday...."
        private string GetDay()
        {
            return DateTime.Now.DayOfWeek.ToString();
        }

        // Gets the current hour between 0 and 23
        private string GetTime()
        {
            return DateTime.Now.Hour.ToString();
        }

        // This method sorts a categoryDataGrid based on the name
        private void SortCategoriesGrid(DataGrid gridToSort)
        {
            List<Category> catList = new List<Category>();

            foreach (Category item in gridToSort.Items)
            {
                catList.Add(item);
            }

            var sortedList = catList.OrderBy(o => o.name).ToList();

            gridToSort.Items.Clear();

            foreach (var item in sortedList)
            {
                gridToSort.Items.Add(item);
            }
        }

        // Replaces single quotes with 2 quotes
        // and replaces ; with ;-- to avoid basic SQL injection
        private string RemoveSQLIssues(string text)
        {
            return text.Replace("'", "''").Replace(";", ";--");
        }
        #endregion

    }
}
