using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Wpf_Telegram_bot_22
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.bw = new BackgroundWorker();
            this.bw.DoWork += this.bw_DoWork; // метод bw_DoWork будет работать асинхронно

        }
        BackgroundWorker bw;





        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            using var cts = new CancellationTokenSource();


            var worker = sender as BackgroundWorker;
            var keyFromTextBox = (List<string>)e.Argument; // получаем ключ из аргументов
            var key = keyFromTextBox[0];
            try
            {
                var botClient = new TelegramBotClient(key); // инициализируем API
                //await botClient.SetWebhookAsync(""); //  убираем старую привязку к вебхуку для бота
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { } // receive all update types
                };
                botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    cancellationToken: cts.Token);

                var me = await botClient.GetMeAsync();

                Debug.WriteLine($"--Start listening for @{me.Username}");

                async Task RunPeriodicSave() //log
                {
                    await Task.Delay(10);
                    var text = $"Start listening for @{me.Username}";
                    ReturnLogToTextBloc(text);
                }
                Task.Run(RunPeriodicSave);


                // Send cancellation request to stop bot
                //  cts.Cancel();


            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Debug.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
            }

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Type != UpdateType.Message)
                    return;
                // Only process text messages
                if (update.Message!.Type != MessageType.Text)
                    return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var me = await botClient.GetMeAsync();
                var message = update.Message;
                Debug.WriteLine($"--Received a '{messageText}' message in chat {chatId} .");


                async Task OutOfThred() //log
                {
                    var textBox = $"Received a '{messageText}' message in chat {chatId} - {me.Username}  ";
                    ReturnLogToTextBloc(textBox);
                }
                Task.Run(OutOfThred);




                if (message.Text == "привет")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Привет как дела?");

                }

                if (message.Text == keyFromTextBox[1])
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, keyFromTextBox[2]);

                }
                if (message.Text == keyFromTextBox[3])
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, keyFromTextBox[4]);

                }

                #region эхо ответ
                // Echo received message text
                /*       Message sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "You said:\n" + messageText,
                            cancellationToken: cancellationToken);*/

                #endregion

                #region повтор с кнопкой и ссвлко на кнопке
                /*      Message message = await botClient.SendTextMessageAsync( 
                      chatId: chatId,
                      text: "Trying *all the parameters* of `sendMessage` method",
                      parseMode: ParseMode.MarkdownV2,
                      disableNotification: true,
                      replyToMessageId: update.Message.MessageId,
                      replyMarkup: new InlineKeyboardMarkup(
                          InlineKeyboardButton.WithUrl(
                              "Check sendMessage method",
                              "https://core.telegram.org/bots/api#sendmessage")),
                      cancellationToken: cancellationToken);*/
                //
                #endregion

            }

            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Debug.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }


        }




        void ReturnLogToTextBloc(string text)
        {
            Dispatcher.Invoke(() => TextRune.Text = text);
        }

        bool buttonControl = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var botToken = @txtKey.Text;
            var question1 = Question1.Text;
            var answer1 = Answer1.Text;
            var question2 = Question2.Text;
            var answer2 = Answer2.Text;
            List<string> listOfTextBlocks = new List<string>() { botToken, question1, answer1, question2, answer2 };

            // получаем содержимое текстового поля txtKey в переменную botToken
            if (botToken != "" && this.bw.IsBusy != true && buttonControl == false)
            {
                this.bw.RunWorkerAsync(listOfTextBlocks); // передаем эту переменную в виде аргумента методу bw_DoWork

                TextRune.Text = "Бот запущен...";
            }
            buttonControl = true;
        }












    }
}
