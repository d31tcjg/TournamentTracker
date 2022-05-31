using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        private List<TeamModel> availableTeams = GlobalConfig.Connection.GetTeams_All();
        private List<TeamModel> selectedTeams = new List<TeamModel>();
        private List<PrizeModel> selectedPrizes = new List<PrizeModel>();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;
            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;
            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = "PlaceName";
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            var team = (TeamModel)selectTeamDropDown.SelectedItem;
            if (team != null)
            {
                availableTeams.Remove(team);
                selectedTeams.Add(team);

                WireUpLists();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // Call the CreatePrizeForm 
            var form = new CreatePrizeForm(this);
            form.Show();
        }

        public void PrizeComplete(PrizeModel model)
        {
            // Get back from the form a PrizeModel
            // Take the PrizeModel and put it into our list of selected prizes. 
            selectedPrizes.Add(model);
            WireUpLists();
            
        }

        public void TeamComplete(TeamModel model)
        {
            selectedTeams.Add(model);
            WireUpLists();
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = new CreateTeamForm(this);
            form.Show();
        }

        private void removeSelectedPlayerButton_Click(object sender, EventArgs e)
        {
            var team = (TeamModel)tournamentTeamsListBox.SelectedItem;
            if (team != null)
            {
                selectedTeams.Remove(team);
                availableTeams.Add(team);
                WireUpLists();
            }
        }

        private void removeSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            var prize = (PrizeModel)prizesListBox.SelectedItem;
            if (prize != null)
            {
                selectedPrizes.Remove(prize);
                WireUpLists();
                //TODO - Create a deletion from Db/TextFile
            }
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Validate data
            decimal fee = 0;
            bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out fee);

            if (!feeAcceptable)
            {
                MessageBox.Show("You need to enter a valid Entry Fee.", caption: "Invalid Fee", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
                

            // Create our tournament model
            var tournament = new TournamentModel();
            
            tournament.TournamentName = tournamentNameValue.Text;
            tournament.EntryFee = fee;

            tournament.Prizes = selectedPrizes;
            tournament.EnteredTeams = selectedTeams;

            // Wire our matchups
            TournamentLogic.CreateRounds(tournament);

            // Create Tournament entry
            // Create all of the prizes entries 
            // Create all of the team entries
            GlobalConfig.Connection.CreateTournament(tournament);

            

        }
    }
}
