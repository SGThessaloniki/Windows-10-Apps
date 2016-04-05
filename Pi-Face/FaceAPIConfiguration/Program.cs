using System;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;


//Project created by Theofilos Spyrou for the Student Guru Thessaloniki Community on April 2016


namespace FaceAPIConfiguration
{
    class Program
    {
        //Face API Properties
        private static string personGroupId = "";           //Enter the group name here
        private static string oxfordKey = "";               //Enter your Project Oxford Face API key here
        private static IFaceServiceClient faceServiceClient;

        //Properties for the creation of a new person
        private static string personName = "";              //Enter the name of the person you want to create here
        private static string photosPath = "";              //Enter the path where the person's photo are located here
        

        static void Main(string[] args)
        {
            faceServiceClient = new FaceServiceClient(oxfordKey);
            Task.Run(async () => { await initialization(personName, photosPath); });

            Console.ReadLine();
        }

        private static async Task initialization(string name, string path)
        {
            try
            {
                //Creates a group with the requested name (the ID and the name of the group don't have to be the same)
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupId);

                //Creates a person in the group we just created with the requested name
                CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, name);

                foreach (string imagePath in Directory.GetFiles(path, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        //Adds a new face for the person we created. It actually uploads each photo of them
                        //to the Project Oxford Server, so that the group can be trained.
                        await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                    }


                    //Training of the group
                    await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                    //Wait untill training ends
                    TrainingStatus trainingStatus = null;
                    while (true)
                    {
                        trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                        //If training stops, exit loop and print its result
                        if (trainingStatus.Status != Status.Running)
                        {
                            Console.WriteLine("Training " + trainingStatus.Status.ToString());
                            break;
                        }

                        await Task.Delay(1000);
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine(ex.ErrorMessage);
            }
        }

    }
}

