using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTeamForm : Form
    {
        private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers = new List<PersonModel>();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();

            callingForm = caller;
            
            //CreateSampleData();

            WireUpLists();
        }

        private void CreateSampleData()
        {
            availableTeamMembers.Add(new PersonModel{FirstName = "Tim", LastName = "Corey"});
            availableTeamMembers.Add(new PersonModel{FirstName = "Sue", LastName = "Smith" });

            selectedTeamMembers.Add(new PersonModel { FirstName = "Jane", LastName = "Smith" });
            selectedTeamMembers.Add(new PersonModel{FirstName = "Bill", LastName = "Jones"});
        }

        private void WireUpLists()
        {
            selectTeamMemberDropDown.DataSource = null;
            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = "FullName";

            teamMembersListBox.DataSource = null;
            teamMembersListBox.DataSource = selectedTeamMembers;
            teamMembersListBox.DisplayMember = "FullName";
        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                var model = new PersonModel()
                {
                    FirstName = firstNameValue.Text,
                    LastName = lastNameValue.Text,
                    EmailAddress = emailValue.Text,
                    CellphoneNumber = cellphoneValue.Text
                };

                var person = GlobalConfig.Connection.CreatePerson(model);
                selectedTeamMembers.Add(person);
                WireUpLists();
                ClearAddMemberTextBoxes();
            }
            else
            {
                MessageBox.Show("You need to fill in all the fields.");
            }
        }

        private void ClearAddMemberTextBoxes()
        {
            firstNameValue.Text = String.Empty;
            lastNameValue.Text = String.Empty;
            emailValue.Text = String.Empty;
            cellphoneValue.Text = String.Empty;
        }

        private bool ValidateForm()
        {
            if (firstNameValue.Text.Length.Equals(0)) return false;
            if (lastNameValue.Text.Length.Equals(0)) return false;
            if (emailValue.Text.Length.Equals(0)) return false;
            if (cellphoneValue.Text.Length.Equals(0)) return false;
            return true;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            var person = (PersonModel)selectTeamMemberDropDown.SelectedItem;
            if (person != null)
            {
                availableTeamMembers.Remove(person);
                selectedTeamMembers.Add(person);
                WireUpLists(); 
            }
            
        }

        private void removeSelectedMemberButton_Click(object sender, EventArgs e)
        {
            var person = (PersonModel)teamMembersListBox.SelectedItem;
            if (person != null)
            {
                selectedTeamMembers.Remove(person);
                availableTeamMembers.Add(person);
                WireUpLists(); 
            }
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel team = new TeamModel();

            team.TeamName = teamNameValue.Text;
            team.TeamMembers = selectedTeamMembers;
            GlobalConfig.Connection.CreateTeam(team);
            callingForm.TeamComplete(team);
            this.Close();

        }
    }
}
