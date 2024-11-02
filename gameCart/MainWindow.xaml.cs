﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace NapierstkiGame
{
    public partial class MainWindow : Window
    {
        private int hiddenCupIndex;
        private Random random;
        private List<Image> cups;
        private bool isShuffling;
        private double shuffleSpeed = 0.3; // Initial speed in seconds
        private const double Cup1StartX = 100;
        private const double Cup2StartX = 200;
        private const double Cup3StartX = 300;
        private const double StartY = 150;

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            cups = new List<Image> { Cup1, Cup2, Cup3 };
            ResetGame();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            hiddenCupIndex = random.Next(0, cups.Count);
            Ball.Visibility = Visibility.Hidden;
            ResultText.Text = "Запоминайте!";
            cups[hiddenCupIndex].Source = new BitmapImage(new Uri("dama.png", UriKind.Relative));
            AnimateCupReturnToOblo(hiddenCupIndex, 2);
        }

        private void AnimateCupReturnToOblo(int cupIndex, double seconds)
        {
            var timer = new System.Timers.Timer(seconds * 1000);
            timer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    cups[cupIndex].Source = new BitmapImage(new Uri("oblo.png", UriKind.Relative));
                    timer.Stop();
                    timer.Dispose();
                    StartShuffleDelay(1);
                });
            };
            timer.Start();
        }

        private void StartShuffleDelay(double seconds)
        {
            var delayTimer = new System.Timers.Timer(seconds * 1000);
            delayTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    delayTimer.Stop();
                    delayTimer.Dispose();
                    StartShuffleAnimation();
                });
            };
            delayTimer.Start();
        }

        private void StartShuffleAnimation()
        {
            ResultText.Text = "Перемешиваем!";
            isShuffling = true;
            EnableCups(false);

            int shuffleSteps = 20;
            int shuffleIndex = 0;

            void ShuffleStep()
            {
                if (shuffleIndex >= shuffleSteps)
                {
                    isShuffling = false;
                    ResultText.Text = "Угадайте, где Дама!";
                    EnableCups(true);
                    return;
                }

                int firstIndex = random.Next(cups.Count);
                int secondIndex;
                do
                {
                    secondIndex = random.Next(cups.Count);
                } while (secondIndex == firstIndex);

                SwapCupsPosition(firstIndex, secondIndex, () =>
                {
                    shuffleIndex++;
                    shuffleSpeed = Math.Max(0.1, shuffleSpeed * 0.9); 
                    ShuffleStep();
                });
            }

            ShuffleStep();
        }

        private void SwapCupsPosition(int firstCupIndex, int secondCupIndex, Action completedAction = null)
        {
            var firstCup = cups[firstCupIndex];
            var secondCup = cups[secondCupIndex];

            double firstCupX = Canvas.GetLeft(firstCup);
            double secondCupX = Canvas.GetLeft(secondCup);

            var firstCupAnimation = new DoubleAnimation(firstCupX, secondCupX, TimeSpan.FromSeconds(shuffleSpeed));
            var secondCupAnimation = new DoubleAnimation(secondCupX, firstCupX, TimeSpan.FromSeconds(shuffleSpeed));

            if (completedAction != null)
            {
                secondCupAnimation.Completed += (s, e) => completedAction();
            }

            firstCup.BeginAnimation(Canvas.LeftProperty, firstCupAnimation);
            secondCup.BeginAnimation(Canvas.LeftProperty, secondCupAnimation);
        }

        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (isShuffling) return;

            if (sender is Image clickedCup)
            {
                int selectedCupIndex = cups.IndexOf(clickedCup);
                EnableCups(false);

                cups[hiddenCupIndex].Source = new BitmapImage(new Uri("dama.png", UriKind.Relative));

                if (selectedCupIndex == hiddenCupIndex)
                {
                    ResultText.Text = "Поздравляем! Вы угадали!";
                    Ball.Visibility = Visibility.Visible;
                }
                else
                {
                    ResultText.Text = "Не угадали! Попробуйте снова.";
                }

                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += (s, args) =>
                {
                    ResetGameAfterGuess();
                    dispatcherTimer.Stop();
                };
                dispatcherTimer.Interval = TimeSpan.FromSeconds(2);
                dispatcherTimer.Start();
            }
        }

        private void EnableCups(bool isEnabled)
        {
            foreach (var cup in cups)
            {
                cup.IsEnabled = isEnabled;
            }
        }

        private void ResetGame()
        {
            hiddenCupIndex = -1;
            Ball.Visibility = Visibility.Hidden;
            ResultText.Text = "Нажмите 'Начать игру', чтобы спрятать сюрприз!";
            EnableCups(false);
            Canvas.SetLeft(Cup1, Cup1StartX);
            Canvas.SetTop(Cup1, StartY);
            Canvas.SetLeft(Cup2, Cup2StartX);
            Canvas.SetTop(Cup2, StartY);
            Canvas.SetLeft(Cup3, Cup3StartX);
            Canvas.SetTop(Cup3, StartY);
            foreach (var cup in cups)
            {
                cup.Source = new BitmapImage(new Uri("oblo.png", UriKind.Relative));
            }
            shuffleSpeed = 0.9;
        }

        private void ResetGameAfterGuess()
        {
            foreach (var cup in cups)
            {
                cup.Source = new BitmapImage(new Uri("oblo.png", UriKind.Relative));
            }
            ResetGame();
        }
    }
}
