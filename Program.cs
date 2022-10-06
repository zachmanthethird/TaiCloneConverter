﻿var filePath=Console.ReadLine().Replace('\\','/');List<string>notes=new();decimal curBPM=-1,mapSVMultiplier=1m,totalCurSV=1m;int currentMeter=4,previewTime=0;string artist=null,audioFileName=null,bgFileName="",charter=null,difficultyName=null,fusVersion="v0.0.7",songTitle=null;var FindValue=(string key,string line)=>line.StartsWith(key)?line.Substring(key.Length).Trim():null;try{if(filePath.EndsWith(".osu")){List<(decimal NextTime,int Meter,decimal Timing,bool Kiai,bool FirstBarline)>currentTimingData=new();string section="";var currentKiai=false;var nextBarline=0m;void AddNote(List<object>note)=>notes.Add(string.Join(',',note));void Barline(decimal curSV){if(totalCurSV>0)AddNote(new(){nextBarline,totalCurSV,NoteType.Barline});nextBarline+=curBPM==0?decimal.MaxValue:60*currentMeter/curBPM;};foreach(var line in File.ReadLines(filePath))if(line.Length>0){var lineData=line.Split(',').ToList();if($"{line.First()}{line.Last()}"=="[]")section=line[1..(line.Length-1)];else switch(section){case"Difficulty":if(decimal.TryParse(FindValue("SliderMultiplier:",line),out var dValue)){mapSVMultiplier=dValue;totalCurSV=dValue;}break;case"Events":if(FindValue("//",line)=="Background and Video events")bgFileName=null;else bgFileName??=lineData[2][1..(lineData[2].Length-1)];break;case"General":audioFileName??=FindValue("AudioFilename:",line);if(int.TryParse(FindValue("PreviewTime:",line),out var iValue))previewTime=Math.Max(iValue,previewTime);break;case"HitObjects":var time=int.Parse(lineData[2])/1000m;while(currentTimingData.Count>0){var timingData=currentTimingData[0];if(curBPM==-1&&timingData.Meter>0){nextBarline=timingData.NextTime;while(timingData.NextTime>time)timingData.NextTime-=60*timingData.Meter/timingData.Timing;currentTimingData[0]=timingData;}if(timingData.NextTime>time)break;if(timingData.Meter>0){while(nextBarline<timingData.NextTime)Barline(totalCurSV);curBPM=timingData.Timing;currentMeter=timingData.Meter;nextBarline=timingData.NextTime;totalCurSV=mapSVMultiplier;}else totalCurSV=timingData.Timing*mapSVMultiplier;if(timingData.Meter>0||timingData.Kiai!=currentKiai){AddNote(new(){nextBarline,curBPM,NoteType.TimingPoint,timingData.Kiai});currentKiai=timingData.Kiai;}if(timingData.FirstBarline)Barline(0);currentTimingData.RemoveAt(0);}while(nextBarline<=time)Barline(totalCurSV);if((8&int.Parse(lineData[3]))>0){AddNote(new(){time,totalCurSV,NoteType.Spinner,int.Parse(lineData[5])/1000m-time});continue;}var finisher=(4&int.Parse(lineData[4]))>0;if((2&int.Parse(lineData[3]))>0)AddNote(new(){time,totalCurSV,NoteType.Roll,curBPM*totalCurSV==0?decimal.MaxValue:decimal.Parse(lineData[7])*int.Parse(lineData[6])*0.6m/curBPM/totalCurSV,finisher});else AddNote(new(){time,totalCurSV,(10&int.Parse(lineData[4]))>0?NoteType.Kat:NoteType.Don,finisher});break;case"Metadata":artist??=FindValue("Artist:",line);charter??=FindValue("Creator:",line);difficultyName??=FindValue("Version:",line);songTitle??=FindValue("Title:",line);break;case"TimingPoints":if(lineData.Count<3)lineData.Add("4");if(lineData.Count<4)lineData.Add("0");if(lineData.Count<5)lineData.Add("0");if(lineData.Count<6)lineData.Add("100");if(lineData.Count<7)lineData.Add("1");if(lineData.Count<8)lineData.Add("0");var sliderVelocity=decimal.Parse(lineData[1]);var uninherited=lineData[6]=="1";currentTimingData.Add((int.Parse(lineData[0])/1000m,uninherited?int.Parse(lineData[2]):0,sliderVelocity==0?decimal.MaxValue:(uninherited?60000:-100)/sliderVelocity,(1&int.Parse(lineData[7]))>0,(8&int.Parse(lineData[7]))>0));break;}}}var folderPath=filePath[..filePath.LastIndexOf('/')];notes.InsertRange(0,new string[]{fusVersion,songTitle,$"{previewTime/1000f}",folderPath,difficultyName,charter,bgFileName,audioFileName,artist});File.WriteAllLines(folderPath+"/TaiCloneConverter.fus",notes);}catch{}enum NoteType{TimingPoint,Barline,Don,Kat,Roll,Spinner}