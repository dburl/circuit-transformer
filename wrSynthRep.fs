module wrSynthRep

//used Modules
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Text.RegularExpressions
open Microsoft.Office.Interop.Excel // to export to Excel

//my Modules
open readReport

//FUNCTIONS DEFINITIONS
// from list create a list wo repetitions
let woRepeatL (lst: string list)=
   let rec checkNxt (forwL: string list) (accSet: string list)=
        let item= List.head forwL
        let ifAlready= List.exists (fun accEl-> accEl=item) accSet
        let newAccSet=
            if ifAlready then 
                accSet
            else
                List.append accSet [item]
        if (List.length forwL)>1 then 
            checkNxt (List.tail forwL) newAccSet
        else 
            newAccSet
   checkNxt lst []

// hopping though the Excel table cell's address
let plusRowColumn (initPos:string) (plusRow: int) (plusCol: int)= //[!] works only with 1 1st letter
    let letter=  String.get initPos 0
    let number= String.sub initPos 1 ((String.length initPos)-1) |> (fun x -> int (x.ToString())) 
    let newLetter= Convert.ToChar(Convert.ToInt32(letter) + plusCol)
    let newNumb= number + plusRow
    newLetter.ToString()+newNumb.ToString()

// create Excel sheets with HW and Freq synthesis results
let synth2Excel (topDir:string)=
    let allRepP= collectSynthRep topDir
    let repRA= allRepP |> List.map (fun p->synRepParse p ) |> Array.ofList // Array of all report records
    let repN= Array.length repRA // number of all synthesis reports and records consequently
    let repeatCirNamA= repRA |> Array.map (fun repR-> repR.cirName) 
    let cirNamA = Array.ofList <| ( repeatCirNamA |> Array.toList |> woRepeatL ) // circuit names without repetition
    let cirN= Array.length cirNamA // number of benchmark circuits
    let repeatTrTypeA= repRA |> Array.map (fun repR-> repR.transT) 
    let trTypeA = Array.ofList <| ( repeatTrTypeA |> Array.toList |> woRepeatL) // transformation types wo repetition
    let transN= Array.length trTypeA // number of transformation types
    // export to Excel
    let app = new ApplicationClass(Visible = true)  // true for debugging -> switch to false later
    let workbookHW = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet) 
    let workbookFrq = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet) 
    let hwSheet = (workbookHW.Worksheets.[1] :?> _Worksheet) //sheet for HW
    let frqSheet = (workbookFrq.Worksheets.[1] :?> _Worksheet)// sheet for Freq
    // create axeses of the table 
    let TLC= "C3"
        //hw
    let nameCol= Array2D.init cirNamA.Length 1 (fun i _ -> cirNamA.[i])
    hwSheet.Range(plusRowColumn TLC 1 0, plusRowColumn TLC cirN 0).Value2 <-nameCol // collumn of cir names
    hwSheet.Range(plusRowColumn TLC 0 1, plusRowColumn TLC 0 transN).Value2 <-trTypeA// row of trans types
        //freq
    frqSheet.Range(plusRowColumn TLC 1 0, plusRowColumn TLC cirN 0).Value2 <- nameCol // collumn of cir names
    frqSheet.Range(plusRowColumn TLC 0 1, plusRowColumn TLC 0 transN).Value2 <-trTypeA // row of transformation types
    //func to understand where insert the report entry
    let repR2Excel (repR:synth_T) =
        let cirNameSeq= Array.findIndex (fun cirNam-> cirNam= repR.cirName) cirNamA
        let transSeq= Array.findIndex (fun cirNam-> cirNam= repR.transT) trTypeA
        (cirNameSeq,transSeq)
    // 
    let tblResHw=   Array2D.init cirN transN (fun i j-> -10.0) // -10 will be re-written
    let tblResFreq= Array2D.init cirN transN (fun i j-> -10.0) // -10 will be re-written
    //
    // fill one Excel cell with record entry
    let fillExcelCell (repR:synth_T)=
        let (nameSeq, transSeq)=repR2Excel repR
        tblResHw.[nameSeq,transSeq]<-   repR.hwUse  // re-writing HW cell
        tblResFreq.[nameSeq,transSeq]<- repR.frq    // re-writng freq cell
    //
    repRA |> Array.iter (fun repR-> fillExcelCell repR)     //fill 2D arrays with entries from all records

    let LCorn=plusRowColumn TLC 1 1//left top corner
    let hwRCorner= plusRowColumn LCorn (cirN-1) (transN-1) //right bottom table corner (HW)
    let freqRCorner=plusRowColumn LCorn (cirN-1) (transN-1)//right bottom table corner (Freq)

    frqSheet.Range(LCorn, freqRCorner).Value2 <- tblResFreq // table to HW sheet
    hwSheet.Range(LCorn,hwRCorner).Value2 <- tblResHw // table to HW sheet
    //TABLE BUILDING IS DONE
    // Additing the chart to one generated sheet -> apply to all of them
        // HW plot
    let chartobjects = (hwSheet.ChartObjects() :?> ChartObjects) 
    let chartobject = chartobjects.Add(400.0, 20.0, 550.0, 350.0)  
    chartobject.Chart.ChartWizard  
        (Title = "Synthesis Results", 
         Source = hwSheet.Range(TLC, hwRCorner), 
         Gallery = XlChartType.xl3DColumnClustered, PlotBy = XlRowCol.xlColumns, 
         SeriesLabels = 1, CategoryLabels = 1, 
         CategoryTitle = "", ValueTitle = "Hardware(LUTs)") 
    //chartobject.Chart.ChartStyle <- 5
        
        // Freq Plot
    let chartobjects = (frqSheet.ChartObjects() :?> ChartObjects) 
    let chartobject = chartobjects.Add(400.0, 20.0, 550.0, 350.0)  
    chartobject.Chart.ChartWizard  
        (Title = "Synthesis Results", 
         Source = frqSheet.Range(TLC, hwRCorner), 
         Gallery = XlChartType.xl3DColumnClustered, PlotBy = XlRowCol.xlColumns, 
         SeriesLabels = 1, CategoryLabels = 1, 
         CategoryTitle = "", ValueTitle = "Frequency(MHz)") 
    //chartobject.Chart.ChartStyle <- 5
    
