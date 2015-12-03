namespace TexasHoldem.AI.SmartPlayer
{
    using System;
    using System.Collections.Generic;
    using TexasHoldem.AI.SmartPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using Logic.Helpers;
    using TexasHoldem.Logic.Players;
    using Logic.Cards;
    using System.Linq;

    public class GoshoTheBot : BasePlayer
    {
        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();
        // calculation implented at start of preflop round
        // not sure if MoneyLeft property is OUR money left?!?!
        public static int mFactor;
        //implemented
        public static bool hasTheButton = false;

        //we should find who has iniative
        public static bool iniative = false;

        ////fixed? ; initialized at start of hand
        public static bool is3bettedPot = false;

        //i know there is a smarter way, but i use it for Post-Flop
        public static bool hasTopPremium = false;

        public override string Name { get; } = "Gosho" + Guid.NewGuid();

        public override void StartGame(StartGameContext context)
        {            
            base.StartGame(context);
        }

        public override void StartHand(StartHandContext context)
        {
           
            is3bettedPot = false;
            hasTheButton = false;
            hasTopPremium = false;
            mFactor = context.MoneyLeft / (context.SmallBlind * 2);
            base.StartHand(context);
        }

        public override void StartRound(StartRoundContext context)
        {            
            base.StartRound(context);
        }
        public override PlayerAction GetTurn(GetTurnContext context)
        {
        
            #region PreFlopLogic            
            if (context.RoundType == GameRoundType.PreFlop)
            {
                //checks if we have the button
                if (context.MyMoneyInTheRound == context.SmallBlind)
                {
                    hasTheButton = true;
                }
                //checks if 3bettedPot; If the pot is big it is!
                if (context.CurrentPot > (context.SmallBlind*4))
                {
                    is3bettedPot = true;
                }

                var playHand = HandStrengthValuation.PreFlop(this.FirstCard, this.SecondCard, hasTheButton);

               
                //http://www.holdemresources.net/h/poker-theory/hune/usage.html

                #region mFactor< 20
                if (mFactor < 20)
                {

                    double nashEquillibriumRatio = 0;

                    if (hasTheButton)
                    {
                        nashEquillibriumRatio = HandStrengthValuation.GetPusherBlindsCount(this.FirstCard, this.SecondCard);
                    }
                    else
                    {
                        nashEquillibriumRatio = HandStrengthValuation.GetCallerBlindsCount(this.FirstCard, this.SecondCard);
                    }

                    //find if we have less money then the effective stack size
                    bool push = context.MoneyLeft <= (nashEquillibriumRatio * (context.SmallBlind* 2));

                    if (push)
                    {
                        return PlayerAction.Raise(context.CurrentMaxBet);
                    }
                    else
                    {
                        if (context.CanCheck)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                }
                #endregion

                #region UnplayableHands
                if (playHand == CardValuationType.Unplayable)
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return PlayerAction.Fold();
                    }
                }
                #endregion

                // raises risky hands only if in position and if it is not a 3betted Pot
                if (playHand == CardValuationType.Risky && context.MyMoneyInTheRound == context.SmallBlind)
                {
                 //   var smallBlindsTimes = RandomProvider.Next(1, 8);
                    return PlayerAction.Raise(context.SmallBlind * 6);
                }

                // folds if not in position
                else if(playHand == CardValuationType.Risky && !hasTheButton)
                {
                    
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return PlayerAction.Fold();
                    }
                }
              
                // if recommended and not in 3bettedPot and a big M RAISES
                if (playHand == CardValuationType.Recommended)
                {
                    if (is3bettedPot && mFactor > 100)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    if (is3bettedPot && mFactor > 200)
                    {
                        return PlayerAction.Fold();
                    }
                    // var smallBlindsTimes = RandomProvider.Next(6, 10);
                    return PlayerAction.Raise(context.SmallBlind * 6);
                }
                //needs refactoring
                if (playHand == CardValuationType.Premium || playHand == CardValuationType.TopPremium)
                {
                    if (playHand == CardValuationType.TopPremium)
                    {
                        hasTopPremium = true;
                        return PlayerAction.Raise(context.SmallBlind * 10);
                    }

                    if (playHand == CardValuationType.Premium)
                    {
                        return PlayerAction.Raise(context.SmallBlind * 10);
                    }                    
                }

            // not sure if this doesn't brake everything
               if (context.CanCheck)
               {
                   return PlayerAction.CheckOrCall();
               }
             // not sure if this doesn't brake everything
                else
                {
                    return PlayerAction.Fold();
                }

            }
            #endregion

            // This doesn't make a difference if it is FLOP, TURN or RIVER!
            #region Post-FlopLogic 
            //List<Card> cards = new List<Card>();
            //for (int i = 0; i < 5; i++)
            //{
            //    cards.Add(this.FirstCard);
            //}
            //List<Card> myCommunityCards = new List<Card>(this.CommunityCards);
            //HandRankType type = HandEvaluator.GetBestHand(cards).RankType;
            //i don't know what I am doing! Trying to get who has initiative; THIS CRASHES!
            //  List<string> lastActions = (List<string>)context.PreviousRoundActions;
            //  var lastPlayer = lastActions[lastActions.Count];
            //  if (lastPlayer == "Gosho Raise")
            //  {
            //      iniative = true;
            //  }
                   
            double currentPotRaise = context.CurrentPot * 0.55;
            int currentPotRaiseInt = (int)currentPotRaise;

       //   if (hasTopPremium)
       //   {
       //       return PlayerAction.Raise(currentPotRaiseInt);
       //   }
       #region Flop
            if (context.RoundType == GameRoundType.Flop)
            {
                List<Card> allCards = new List<Card>(this.CommunityCards);
                allCards.Add(this.FirstCard);
                allCards.Add(this.SecondCard);
                HandRankType type = HandEvaluator.GetBestHand(allCards).RankType;
                if ((int)type >= (int)HandRankType.Pair && context.CanCheck)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }

                if ((int)type == (int)HandRankType.Pair && (context.MoneyToCall < currentPotRaise))
                {
                    return PlayerAction.CheckOrCall();
                }

                if ((int)type >= (int)HandRankType.TwoPairs)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }
                else
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    else
                    {
                        //always call SmallBlinds/BigBlinds!
                       if (context.MoneyToCall <= (context.SmallBlind * 2))
                       {
                           return PlayerAction.CheckOrCall();
                       }

                        return PlayerAction.Fold();
                    }
                }
            }
            #endregion
            #region Turn    
            if (context.RoundType == GameRoundType.Turn)
            {
                List<Card> allCards = new List<Card>(this.CommunityCards);
                allCards.Add(this.FirstCard);
                allCards.Add(this.SecondCard);
                HandRankType type = HandEvaluator.GetBestHand(allCards).RankType;
                if ((int)type >= (int)HandRankType.Pair && context.CanCheck)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }

                if ((int)type == (int)HandRankType.Pair && (context.MoneyToCall < currentPotRaise))
                {
                    return PlayerAction.CheckOrCall();
                }

                if ((int)type >= (int)HandRankType.TwoPairs)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }
                else
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    else
                    {
                        //always call SmallBlinds/BigBlinds!
                        if (context.MoneyToCall < (context.SmallBlind * 2))
                        {
                            return PlayerAction.CheckOrCall();
                        }

                        return PlayerAction.Fold();
                    }
                }
            }
            #endregion

            

            #region River
            if (context.RoundType == GameRoundType.River)
            {
                List<Card> allCards = new List<Card>(this.CommunityCards);
                allCards.Add(this.FirstCard);
                allCards.Add(this.SecondCard);
                HandRankType type = HandEvaluator.GetBestHand(allCards).RankType;
                if ((int)type >= (int)HandRankType.Pair && context.CanCheck)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }

                if ((int)type == (int)HandRankType.Pair && (context.MoneyToCall < currentPotRaise))
                {
                    return PlayerAction.CheckOrCall();
                }

                if ((int)type >= (int)HandRankType.TwoPairs)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }
                else
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    else
                    {
                        //always call SmallBlinds/BigBlinds!
                        if (context.MoneyToCall < (context.SmallBlind * 2))
                        {
                            return PlayerAction.CheckOrCall();
                        }

                        return PlayerAction.Fold();
                    }
                }
            }
            #endregion

            #region OldRandomLogic
            //redundant logic
            //   else
            //   {
            //       //always call SmallBlinds!
            //       if (context.MoneyToCall == context.SmallBlind)
            //       {
            //           return PlayerAction.CheckOrCall();
            //       }
            //
            //       int ourCurrentStack = context.MoneyLeft;
            //       //if the pot is bigger than our money= ALL-IN BABY!
            //       if (context.CurrentPot > context.MoneyLeft && context.MoneyLeft > 0)
            //       {
            //           return PlayerAction.Raise(ourCurrentStack);
            //       }
            //
            //
            //       var chanceForAction = RandomProvider.Next(1, 50);
            //
            //       //always bets
            //       if (iniative && context.CanCheck)
            //       {
            //           return PlayerAction.Raise(currentPotRaiseInt);
            //       }
            //
            //       if (chanceForAction <= 35)
            //       {
            //           return PlayerAction.Raise(currentPotRaiseInt);
            //       }
            //
            //       if (chanceForAction > 35)
            //       {
            //           return PlayerAction.CheckOrCall();
            //       }
            //
            #endregion
            if (context.CanCheck)
                 {
                     return PlayerAction.CheckOrCall();
                 }
            
                 else
                 {
                     return PlayerAction.Fold();
                 }
       
           
        }
        #endregion

    }
}
