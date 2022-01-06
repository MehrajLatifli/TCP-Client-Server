using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using TCP_Client_Server.Commands;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TCP_Client_Server.View_Models
{
    public class MainViewModel : BaseViewModel
    {

        TcpListener server;
        NetworkStream serverStream;
        TcpClient client;
        NetworkStream clientStream;

        public MainWindow MainWindows { get; set; }
        public RelayCommand SendCommand { get; set; }
        public RelayCommand StartCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }


        public string Receive;
        public string TextToSend;


        bool changeposition = false;

        DispatcherTimer timer = new DispatcherTimer();




        public MainViewModel()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            timer.Tick += Timer_Tick;




            LoadedCommand = new RelayCommand((sender) =>
            {

                timer.Start();

            });

            StartCommand = new RelayCommand((sender) =>
            {

                Task.Run(() =>
                {

                    changeposition = true;

                

                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.connectbutton.IsEnabled=false;
                        });

                        if (server != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindows.ChatScreenTextbox.Text += "\n Already running server.";
                            });
                            return;
                        }

                        try
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                server = new TcpListener(IPAddress.Parse(MainWindows.ServerIpTextbox.Text), int.Parse(MainWindows.ServerPortTextBox.Text));

                                MainWindows.ChatScreenTextbox.Text += "\n Started server.";

                                MainWindows.Title = "Server";

                            }));

                            Task.Run(() =>
                            {
                                ServerStart(server);
                            });
                        }
                        catch
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindows.ChatScreenTextbox.Text += "\n Failed to start server. (Is your port correct?)";
                            });
                            return;
                        }







                    }
                    catch (Exception ex)
                    {

                        System.Windows.MessageBox.Show($"{ex.Message}");

                    }

                });
            });


            ConnectCommand = new RelayCommand((sender) =>
            {
                Task.Run(() =>
                {

                    changeposition = false;


                    try
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.startbutton.IsEnabled = false;
                        });


                        if (client != null)
                        {


                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindows.ChatScreenTextbox.Text += "\n Client already connected.";
                            });
                            return;
                        }

                        try
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (client==null)
                                {
                                    try
                                    {

                                        client = new TcpClient(MainWindows.ServerIpTextbox.Text, int.Parse(MainWindows.ServerPortTextBox.Text));
                                        App.Current.Dispatcher.Invoke(() =>
                                        {
                                            MainWindows.ChatScreenTextbox.Text += "\n Client connected.";

                                            MainWindows.Title = "Client";
                                        });
                                        return;

                                    }
                                    catch (Exception)
                                    {


                                    }
                                }

                          
                            }));

                    

                            Task.Run(() =>
                            {
                                ClientStart(client);
                            });

                        }
                        catch(Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindows.ChatScreenTextbox.Text += "\n Failed to connect client. (Is your port correct?)";
                            });

                            System.Windows.MessageBox.Show($"{ex.Message}");

                        }




                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"{ex.Message}");

                    }






                });
            });


            SendCommand = new RelayCommand((sender) =>
            {
                if (changeposition == true)
                {

                    if (server == null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Server not running.";
                        });

                        return;
                    }

                    else
                    {

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Server: " + MainWindows.MessageTextBox.Text;

                            byte[] data = Encoding.ASCII.GetBytes(MainWindows.MessageTextBox.Text);
                            serverStream.Write(data, 0, data.Length);
                        });

                    }
                }
                else if (changeposition == false)
                {
                    if (client == null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Client not running.";
                        });

                        return;
                    }

                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Client: " + MainWindows.MessageTextBox.Text;

                            byte[] data = Encoding.ASCII.GetBytes(MainWindows.MessageTextBox.Text);
                            clientStream.Write(data, 0, data.Length);
                        });

                    }
                }

            });
        }

        private void ClientStart(TcpClient client_)
        {
            try
            {
                    TcpClient client = (TcpClient)client_;
                if (client != null)
                {

                    byte[] bytes = new byte[1024];
                    clientStream = client.GetStream();


                    int i;

                    while ((i = clientStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        string data = Encoding.ASCII.GetString(bytes, 0, i);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Server: " + data;
                        });


                    }

                    clientStream.Close();
                    client.Close();
                }
                else
                {
                    System.Windows.MessageBox.Show($"First run Server");
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}");

            }

        }

        private void ServerStart(TcpListener server_)
        {
            try
            {
                TcpListener server = (TcpListener)server_;

                server.Start();

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();



                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindows.ChatScreenTextbox.Text += "\n Client connected.";
                    });


                    byte[] bytes = new byte[1024];

                    serverStream = client.GetStream();

                    int i;

                    while ((i = serverStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        string data = Encoding.ASCII.GetString(bytes, 0, i);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindows.ChatScreenTextbox.Text += "\n Client: " + data;
                        });

                    }

                    serverStream.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}");

            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {

                MainWindows.ServerIpTextbox.Text = "127.0.0.1";
                MainWindows.ServerPortTextBox.Text = "8080";


                MainWindows.ClientIpTextbox.Text = "127.0.0.1";
                MainWindows.ClientPortTextbox.Text = "8081";

            }));

        }


    }


}
