//@version=5
//-------------------------------------------------------------------------------------------------------------------------------------------
//  Script Name:    GPX-S-Stoboss v1.0 (STOBOSS) Strategy
//  Author     :    Guy Pedashenko      Original creation date: 25-May-24       Latest version date: 30-May-24
//  Version    :    v1.49
//  Description:    Stochastic @ Boundaries + T-Bars with new 45% head, //                  
//                                                  
//-------------------------------------------------------------------------------------------------------------------------------------------
strategy(title="STOBOSS-S-v1.49", overlay=true)
import TradingView/Strategy/3
import PineCoders/Time/3

// Strategy Parameters             
float RewardFactor = 2.2         // Risk:Reward ratio (Reward Number)
float RiskFactor = 1           // Risk based on size of previous bar
// Exp-01 - A-STOBOSS List 0.5:5 :: 

// Exp-01 - FOREX List 0.5:3 worked well for many 3-17%  Need to do the full XLS analysis of these - but early indications of a good strategy!!
// Exp-01 - FOREX List 0.7:4 worked well for many 5-15%
// Exp-03 - FOREX List 0.5:5 worked well for many 5-13%  Maybe many more at the 10% mark.
// Exp004 - Try swaping K ro D on StochCross ???!!
// Generally this seems to work.  If managed manually - many 1:5 trades or more possible.  And so simple.
// Do a version for some of my favourite Stocks (CBA ++)
// Do some backtesting with Manual Trade Management on a couple of 5% roi ones from Pine and a couple of 15%+ ones  See what it looks like.
// Develop a simple k out of the 80-20 middle zone (Watch Time), but last bar wasn't (so not getting annoying multiple signals for days) + k comes bakc into the mid zone (Action Time)
//       Use Lables (Stoch [Orange]  / Stoch [Green])

float SLFactor = 0.55          // SL Factor for chaning it based on TBAR size
int iContracts = 15000
bool ThruBarYes = false
backtestStartDate = timestamp(2023,05,21)    
backtestEndDate = timestamp(2024,05,21)

// Strategy Variables
nowTime = time
inTradeWindow =  (nowTime >= backtestStartDate and nowTime <= backtestEndDate)
longCondition = false
shortCondition = false
bool LowTBar = false
bool HighTBar = false
bool LowStochCross = false
bool HighStochCross = false
bool LowBollinger = false
bool HighBollinger = false

bool FollowThrough = false          // Follow Through bar - post TBar flag
float SLPips = 0                    // Pips from Entry price to SL
float SLPrice = 0                   // Actual SL Price
float TPPips = 0                    // Pips from Entry price to TP
float TPPrice = 0                   // Actual TP Price
float EntryPrice = 0                // Entry Price
float stopLoss = 0
float takeProfit = 0
float fATR = 0
float gpfSLATR = 0
float fEMA50 = ta.ema(close,50)
float AerialSize = 0   
float FTBARStrength = 0
float TBARSize = 0
float gpfShoulderGap = 0
int PipFactor = 0
string TradeLabelText = "Trade"     // Trade Data Label text
int Direction = 0
float TBarGainPerc = 0
string SecrurityCode = str.substring(syminfo.ticker,0,6) 
PipFactor := 100000                 // Default PipFactor
float TBAR4Risk = 0
float TBAR4Reward = 0

if (SecrurityCode == "AUDCAD")
    PipFactor := 100000    
    iContracts := 1500000
    //  iNumberOfBars := 10 
    //  RewardFactor := 1.1
if (SecrurityCode == "AUDCHF")
    PipFactor := 100000    
    iContracts := 1700000
    //  iNumberOfBars := 5
    //  RewardFactor := 1.1
if (SecrurityCode == "AUDJPY")
    PipFactor := 1000    
    iContracts := 60000
    //RewardFactor := 3         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 20
    //  RewardFactor := 3
if (SecrurityCode == "AUDNZD")
    PipFactor := 100000   
    iContracts := 5000000
    //  iNumberOfBars := 10
    //  RewardFactor := 2.85
if (SecrurityCode == "AUDUSD")
    PipFactor := 100000    
    iContracts := 9500000
    //RewardFactor := 2         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 25  
    //  RewardFactor := 1.65 
if (SecrurityCode == "BTCUSD")
    PipFactor := 100      
    iContracts := 4
   // RewardFactor := 4         // Risk:Reward ratio (Reward Number)
   //  iNumberOfBars := 7
    //  RewardFactor := 3
if (SecrurityCode == "CADCHF")
    PipFactor := 100000   
    iContracts := 5000000
    //  iNumberOfBars := 20
    //  RewardFactor := 5
if (SecrurityCode == "CADJPY")
    PipFactor := 1000   
    //RewardFactor := 4.2         // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    iContracts := 45000
    //  iNumberOfBars := 10
    //  RewardFactor := 1.25 
if (SecrurityCode == "CHFJPY")
    PipFactor := 1000      
    iContracts := 28000
    //RewardFactor := 2.7       // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 12
    //  RewardFactor := 2.5
if (SecrurityCode == "EURAUD")
    PipFactor := 100000    
    iContracts := 4000000
    //RewardFactor := 2.5         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 17
    //  RewardFactor := 1.65
if (SecrurityCode == "EURCAD")
    PipFactor := 100000    
    iContracts := 5000000
    //  iNumberOfBars := 15
    //  RewardFactor := 6.5
if (SecrurityCode == "EURCHF")
    PipFactor := 100000    
    iContracts := 10000000
    //RewardFactor := 2.7         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 5
    //  RewardFactor := 3.5
if (SecrurityCode == "EURGBP")
    PipFactor := 100000    
    iContracts := 1500000
    //  iNumberOfBars := 20
    //  RewardFactor := 1.5
if (SecrurityCode == "EURJPY")
    PipFactor := 1000    
    iContracts := 30000
   // RewardFactor := 3.5         // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 10
    //  RewardFactor := 2.5
if (SecrurityCode == "EURNZD")
    PipFactor := 100000    
    iContracts := 1660000
    //  iNumberOfBars := 7
    //  RewardFactor := 1.5
if (SecrurityCode == "EURUSD")
    PipFactor := 100000    
    iContracts := 9000000
    //RewardFactor := 3        // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 15
    //  RewardFactor := 1.25
if (SecrurityCode == "GBPAUD")
    PipFactor := 100000    
    iContracts := 3500000
    //RewardFactor := 2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 35
    //  RewardFactor := 2.2
if (SecrurityCode == "GBPCAD")
    PipFactor := 100000    
    iContracts := 700000
    //  iNumberOfBars := 30
    //  RewardFactor := 4.4
if (SecrurityCode == "GBPCHF")
    PipFactor := 100000    
    iContracts := 1000000
    //  iNumberOfBars := 28
    //  RewardFactor := 6.5
if (SecrurityCode == "GBPJPY")
    PipFactor := 1000    
    iContracts := 25000
    //RewardFactor := 3        // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 20
    //  RewardFactor := 1.81
if (SecrurityCode == "GBPNZD")
    PipFactor := 100000    
    iContracts := 3500000
    //RewardFactor := 1.6       // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 5
    //  RewardFactor := 1.7
if (SecrurityCode == "GBPUSD")
    PipFactor := 100000    
    iContracts := 5000000
    //RewardFactor := 6        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 7
    //  RewardFactor := 1.7
if (SecrurityCode == "USDCAD")
    PipFactor := 100000      
    iContracts := 6000000
   // RewardFactor := 2.5        // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 12
    //  RewardFactor := 1.55
if (SecrurityCode == "NZDCAD")
    PipFactor := 100000      
    iContracts := 1400000
    //  iNumberOfBars := 5
    //  RewardFactor := 2.5
if (SecrurityCode == "NZDCHF")
    PipFactor := 100000      
    iContracts := 1800000
    //  iNumberOfBars := 5
    //  RewardFactor := 1.72
if (SecrurityCode == "NZDJPY")
    PipFactor := 1000      
    iContracts := 14000
    //  iNumberOfBars := 35
    //  RewardFactor := 3.7
if (SecrurityCode == "NZDUSD")
    PipFactor := 100000     
    iContracts := 10000000
    //RewardFactor := 2.2        // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 20
    //  RewardFactor := 1.66
if (SecrurityCode == "USDCHF")
    PipFactor := 100000      
    iContracts := 8000000
    //RewardFactor := 1.5         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 14
    //  RewardFactor := 2.5
if (SecrurityCode == "USDJPY")
    PipFactor := 1000      
    iContracts := 40000
    //RewardFactor := 2.2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 15   //15,1.25  = 230K
    //  RewardFactor := 1.25 
if (SecrurityCode == "XAUUSD")
    PipFactor := 100        // PIP factor for GOLD
    iContracts := 4000
    //RewardFactor := 3.5        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 15
    //  RewardFactor := 1.65
if (SecrurityCode == "XAGUSD") 
    PipFactor := 1000     // PIP factor for SILVER
    iContracts := 100000
    //RewardFactor := 2.2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 10
    //  RewardFactor := 1.65
if (SecrurityCode == "NAS100") 
    PipFactor := 10     
    //SLFactor := 1
    iContracts := 200
   // RewardFactor := 2.2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 4
    //  RewardFactor := 1.65
if (SecrurityCode == "GER40") 
    PipFactor := 10     
    //SLFactor := 1.3  
    iContracts := 250
    //RewardFactor := 2.2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 5
    //  RewardFactor := 1.9
if (SecrurityCode == "US30") 
    PipFactor := 10     
   // SLFactor := 1 
    iContracts := 250
    //RewardFactor := 2.2         // Risk:Reward ratio (Reward Number)
    //  iNumberOfBars := 7
    //  RewardFactor := 1.91
if (SecrurityCode == "UK100") 
    PipFactor := 10    
   // SLFactor := 1 
    iContracts := 620
    //  iNumberOfBars := 15
    //  RewardFactor := 1.95
if (SecrurityCode == "AUS200") 
    PipFactor := 10     
    //SLFactor := 1 
    iContracts := 640
    //RewardFactor := 2.3        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    //  iNumberOfBars := 17
    //  RewardFactor := 2.1
if (SecrurityCode == "JPN225") 
    PipFactor := 10     
    //SLFactor := 1
    iContracts := 100
   // RewardFactor := 1.2
    //  iNumberOfBars := 20
if (SecrurityCode == "CATTLE") 
    PipFactor := 100000     
   // SLFactor := 1
    iContracts := 2700000
    //  iNumberOfBars := 30
    //RewardFactor := 9
    //RewardFactor := 5
if (SecrurityCode == "COCOA") 
    PipFactor := 10    
    //SLFactor := 1
    iContracts := 170
    //  iNumberOfBars := 5
    //  RewardFactor := 2.1
if (SecrurityCode == "COFFEE")   // Alternative for Low trades better profit is 25, 4
    PipFactor := 100    
    //SLFactor := 1
    iContracts := 8000
    //RewardFactor := 5
    //  iNumberOfBars := 5
if (SecrurityCode == "COPPER") 
    //  iNumberOfBars := 7
    //  RewardFactor := 3
    PipFactor := 10000    
    //SLFactor := 1
    iContracts := 100000
if (SecrurityCode == "CORN") 
    //  iNumberOfBars := 7
    //RewardFactor := 3.5
    PipFactor := 10000   
    //SLFactor := 1
    iContracts := 300000
if (SecrurityCode == "NATGAS") 
    //  iNumberOfBars := 20
    //RewardFactor := 2.3
    PipFactor := 1000    
    //SLFactor := 1
    iContracts := 350000
if (SecrurityCode == "OATS") 
    //  iNumberOfBars := 30
    //RewardFactor := 4
    PipFactor := 10000    
   // SLFactor := 1
    iContracts := 350000
if (SecrurityCode == "SOYBEA")   // Check that SecrurityCode string is OK for this!!!
    //  iNumberOfBars := 7
    //  RewardFactor := 2.5
    //RewardFactor := 2        // Risk:Reward ratio (Reward Number)
    //SLFactor := 0.15          // SL Factor for chaning it based on ATR
    PipFactor := 10   
    //SLFactor := 1
    iContracts := 2000
if (SecrurityCode == "WHEAT") 
    //  iNumberOfBars := 15
    //  RewardFactor := 2.6
    PipFactor := 10    
    //SLFactor := 1
    iContracts := 600

// ------ Stochastic set up --------
periodK = input.int(9, title="%K Length", minval=1)     // Stochastic with Hayden's recommended settings 9,4,4
smoothK = input.int(4, title="%K Smoothing", minval=1)
periodD = input.int(4, title="%D Smoothing", minval=1)
k = ta.sma(ta.stoch(close, high, low, periodK), smoothK)
d = ta.sma(k, periodD)
//  ------ Bollinger Band setup  ------ 
length = input.int(20, minval=1)
maType = input.string("SMA", "Basis MA Type", options = ["SMA", "EMA", "SMMA (RMA)", "WMA", "VWMA"])
src = input(close, title="Source")
mult = input.float(2.0, minval=0.001, maxval=50, title="StdDev")

ma(source, length, _type) =>
    switch _type
        "SMA" => ta.sma(source, length)
        "EMA" => ta.ema(source, length)
        "SMMA (RMA)" => ta.rma(source, length)
        "WMA" => ta.wma(source, length)
        "VWMA" => ta.vwma(source, length)

basis = ma(src, length, maType)
dev = mult * ta.stdev(src, length)
upperBB = basis + dev
lowerBB = basis - dev
offset = input.int(0, "Offset", minval = -500, maxval = 500, display = display.data_window)
plot(basis, "Basis", color=#FF6D00, offset = offset)
p1 = plot(upperBB, "Upper", color=#2962FF, offset = offset)
p2 = plot(lowerBB, "Lower", color=#2962FF, offset = offset)
fill(p1, p2, title = "Background", color=color.rgb(33, 150, 243, 95))

fATR := ta.atr(10)                   // Pure ATR based on iNuimberOfBars
candleHeight = math.abs(high-low)
candleBody = math.abs(open-close)
barlow = math.abs(low)
barhigh = math.abs(high)

// StochCross calculation - Really Stoch Comming back into the Zone
if k > 20 and k[1] <=20 
    LowStochCross := true
if k < 80 and k[1] >= 80 
    HighStochCross := true

// Check Bollinger Band edge touch (or Close)   - would need to work out % closeAtEndOfSession
if low <= lowerBB or low[1] <= lowerBB[1] or low[2] <= lowerBB[2]
    LowBollinger := true
if high >= upperBB or high[1] >= upperBB[1] or high[2] >= upperBB[2]
    HighBollinger := true

if LowStochCross and LowBollinger
    TBAR4Risk := candleHeight * RiskFactor
    TBAR4Reward := TBAR4Risk * RewardFactor
    EntryPrice := high + (5/PipFactor)    
    SLPips := TBAR4Risk  
    TPPips := TBAR4Reward
    TPPrice := EntryPrice + TPPips
    SLPrice := EntryPrice - SLPips 
    longCondition := true

if HighStochCross and HighBollinger
    TBAR4Risk := candleHeight * RiskFactor
    TBAR4Reward := TBAR4Risk * RewardFactor
    EntryPrice := low  - (5/PipFactor)  
    SLPips := TBAR4Risk  
    TPPips := TBAR4Reward
    TPPrice := EntryPrice - TPPips
    SLPrice := EntryPrice + SLPips 
    shortCondition := true

// Calculate Position Size (iContracts)
float long_diff = EntryPrice - SLPrice
float long_tp = EntryPrice + (1 * long_diff)
float balance = (strategy.initial_capital + strategy.netprofit)
float bal_limited = (balance > 0 ? balance : 0)
float bal_risked = 0.01 * bal_limited

//float pos_size = (bal_risked / long_diff)
iContracts := (int((bal_risked * PipFactor) / (long_diff * PipFactor)) )
if iContracts < 0 
    iContracts := iContracts * -1

if (longCondition and inTradeWindow and strategy.position_size == 0) //and bullishEMAAlignment
    Direction := 1   
    stopLoss := SLPrice  
    takeProfit := TPPips * PipFactor
    strategy.order("long", strategy.long, iContracts, stop=EntryPrice)
    strategy.exit("exit", "long", stop=stopLoss, profit=takeProfit)
   // Strategy.closeAtEndOfSession("long")
   //TradeLabelText := str.format("Buy: {0,number,#.####} [{1,number,#.####}:{2,number,#.####}]", EntryPrice, fATR,takeProfit/PipFactor)
    //TradeLabelText := str.substring(syminfo.ticker,0,3)
    //label.new(bar_index,  lowestLows-(fATR*5), text="Buy", color=color.rgb(6, 82, 3), style=label.style_label_up,textcolor=color.rgb(244, 248, 244))

if (shortCondition and inTradeWindow and strategy.position_size == 0) //and bearishEMAAlignment
    Direction := -1   
    stopLoss := SLPrice 
    takeProfit := TPPips * PipFactor
    //takeProfit := EntryPrice - (SLPips * RewardFactor)
    strategy.order("short", strategy.short, iContracts, stop=EntryPrice)
    strategy.exit("exit", "short", stop=stopLoss, profit=takeProfit)
    //TradeLabelText := str.format("Sell: {0,number,#.####} [{1,number,#.####}:{2,number,#.####}]", EntryPrice, fATR,takeProfit/PipFactor)
    //label.new(bar_index,  lowestLows+(fATR*5), text="Sell", color=color.rgb(128, 12, 8) , style=label.style_label_down,textcolor=color.rgb(236, 236, 228))

if ((longCondition[1] or shortCondition[1]) and strategy.position_size == 0)   // Cancle any pending orders that did not trigger.
    strategy.cancel_all()

if LowTBar[1]
    if high - high[1] > 0
        TBarGainPerc := ((high - high[1]) / candleHeight[1])*100
if HighTBar[1]      
    if low[1] - low > 0
        TBarGainPerc := ((low[1] - low) / candleHeight[1])*100
if LowTBar[2] and TBarGainPerc[1] > 0 and high > high[1] and low > low[1]
    TBarGainPerc := ((high - high[2]) / candleHeight[2])*100
if HighTBar[2] and TBarGainPerc[1] > 0 and low < low[1] and high < high[1]
    TBarGainPerc := ((low[2] - low) / candleHeight[2])*100

// See Data Window variable for Debugging
//plot(EntryPrice, "EntryPrice",color=na)
//plot(stopLoss, "SL",color=na)
//plot(gpfSLATR, "SL-Pips",color=na)
//plot(EntryPrice+(takeProfit/PipFactor), "TP Buy",color=na)
//plot(EntryPrice-(takeProfit/PipFactor), "TP Sell",color=na )   //EntryPrice-(takeProfit/PipFactor)
//plot(gpfSLATR * RewardFactor, "TP-Pips",color=na)
//plot(RewardFactor, "Reward Factor",color=na)
//plot(PipFactor, "Pip Factor",color=na)
//plot(EntryPrice-fEMA50, "Dist2EMA50",color=na)
//plot(ta.ema(close,iNumberOfBars),color=color.rgb(223, 78, 194))
//plot(gpfShoulderGap, "ShoulderGap",color=na)

// Plot signals to chart
plotshape(longCondition , title="LowStoch", location=location.belowbar, color=color.rgb(223, 78, 194), style=shape.labelup, text="Buy" , textcolor=color.white, size=size.small)
plotshape(shortCondition,  title="HighStoch",location=location.abovebar, color=color.rgb(223, 78, 194), style=shape.labeldown, text="Sell" , textcolor=color.white, size=size.small)
//plotshape(FollowThrough,  title="ThurBar",location=location.bottom, color=color.rgb(78, 177, 223), style=shape.labelup, text="FT" , textcolor=color.white, size=size.small)

plot(Direction, "Direction",color=na)
plot(fATR, "ATR",color=na)
plot(candleHeight, "Candle Size",color=na)
plot(TBARSize, "TBARSize%ATR",color=na)
plot(TBarGainPerc, "TBarGain%",color=na)
//plot(FTBARStrength, "FTBarStrength",color=na)
plot(AerialSize, "AerialSize%",color=na)
plot(iContracts, "iContracts",color=na)
plot(balance, "balance",color=na)
plot(bal_limited, "bal_limited",color=na)
plot(bal_risked, "bal_risked",color=na)
plot(EntryPrice, "EntryPrice",color=na)
plot(SLPrice, "SLPrice",color=na)
plot(long_diff, "long_diff",color=na)

// Send alerts
tradeAlert = FollowThrough 
//alertcondition(tradeAlert, title="TBar+Follow", message="TBar + Follow Through Trade Alert !")