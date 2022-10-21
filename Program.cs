﻿Queue<string>files=new();files.Enqueue(Console.ReadLine().Replace('\\','/'));while(files.Count>0){Action<List<string>,string>EachLine;List<(decimal Time,bool Inherited,int Meter,decimal Value,int Kiai,bool FirstBarline)>currentTimingData=new();List<string>measures=new(),notes=new();decimal beatLength=-1,currentBPM=-1,mapSV=1,nextBarline=0,od=0,offset=0,startingBPM=120,timingTime=0,totalSV=1;int kiai=0,meter=4,previewTime=0;string filePath=files.Dequeue(),artist=null,audioFileName=null,bgFileName="",charter=null,difficultyName=null,extension=filePath[^4..],folderPath=filePath[..filePath.LastIndexOf('/')],fusVersion="v0.0.9",songTitle=null;var FindValue=(string key,string line)=>line.StartsWith(key)?line[key.Length..].Trim():null;void Barline(decimal currentSV){if(currentSV>0)notes.Add($"{nextBarline},{currentSV},{(int)NoteType.Barline}");nextBarline+=meter;};switch(extension){case".fus":(int W,uint N,uint D,decimal Key)Fraction(decimal beats){int w=(int)Math.Floor(beats);beats%=1;uint ld=1,ln=0,max=47,md,mn,ud=1,un=1;while(true){uint weight=1;while(ln+un*weight<=(ld+ud*weight)*beats&&ld+ud*(weight*2-1)<max)weight*=2;for(int i=(int)Math.Log2(weight);i>0;i--){uint value=(uint)Math.Pow(2,i-1);if(ln+un*(weight-value)>(ld+ud*(weight-value))*beats)weight-=value;}ld+=ud*(weight-1);ln+=un*(weight-1);if((decimal)ln/ld==beats){md=ld;mn=ln;break;}weight=1;while(un+ln*weight>=(ud+ld*weight)*beats&&ud+ld*(weight*2-1)<max)weight*=2;for(int i=(int)Math.Log2(weight);i>0;i--){uint value=(uint)Math.Pow(2,i-1);if(un+ln*(weight-value)<(ud+ld*(weight-value))*beats)weight-=value;}ud+=ld*(weight-1);un+=ln*(weight-1);if((decimal)un/ud==beats){md=ud;mn=un;break;}if(ld+ud>=max){var u=Math.Abs(beats-(decimal)un/ud)<Math.Abs(beats-(decimal)ln/ld);md=u?ud:ld;mn=u?un:ln;break;}}if((mn,md)==(1,1)){mn=0;w+=1;}return(w,mn,md,w+(decimal)mn/md);}SortedDictionary<decimal,(List<string>Commands,int Type,int W,uint N,uint D)>measure=new(),nextMeasure=new();decimal currentBarline=0,extraBeats=0;int lineIndex=0;ulong GCF(ulong i,ulong n)=>n==0?i:GCF(n,i%n);ulong LCM(uint d,ulong lcm)=>lcm/GCF(lcm,d)*d;ulong lcm=1;void SVCheck(decimal currentSV,decimal time){if(currentSV!=totalSV)currentTimingData.Add((time,true,0,currentSV,kiai,false));totalSV=currentSV;}EachLine=(lineData,line)=>{lineIndex++;switch(lineIndex){case 1:if(line!=fusVersion)Environment.Exit(0);return;case 2:songTitle=line;return;case 3:int.TryParse(line,out previewTime);return;case 4:decimal.TryParse(line,out od);return;case 5:folderPath=line;return;case 6:difficultyName=line;return;case 7:charter=line;return;case 8:bgFileName=line;return;case 9:audioFileName=line;return;case 10:artist=line;return;}if(lineData.Count<3)return;decimal.TryParse(lineData[0],out var beatTime);decimal.TryParse(lineData[1],out var currentSV);currentSV/=mapSV;Enum.TryParse(typeof(NoteType),lineData[2],out var noteType);(int w,var n,var d,var key)=Fraction(beatLength==0?decimal.MaxValue:(extraBeats+beatTime-(currentBarline-timingTime)/beatLength));measure.TryGetValue(key,out var value);value.Commands??=new();decimal length=0,time=timingTime+beatTime*beatLength;int finisher=0;var note=$"256,192,{Math.Round(time)},";switch(noteType){case NoteType.Barline:note="";/*TODO:osuMeter*/if(beatTime==nextBarline){if(beatTime==0){currentTimingData.Remove((timingTime,false,meter,currentBPM,kiai,true));currentTimingData.Add((timingTime,false,meter,currentBPM,kiai,false));}nextBarline+=meter;}else{/*TODO:osuDecimalCompatibility*/currentTimingData.Add((time,false,meter,currentBPM,kiai,false));nextBarline=beatTime+meter;totalSV=mapSV;}SVCheck(currentSV,time);var beats=beatLength==0?decimal.MaxValue:(extraBeats+(time-currentBarline)/beatLength);if(meter!=Math.Ceiling(beats)){meter=Math.Max(1,(int)Math.Ceiling(beats));measures.Add($"#MEASURE {meter}/4");}ulong measureLength=0,nextLCM=1;var measureLine="";if(lcm>10000)Console.WriteLine(lcm);foreach(var measureNote in measure){value=measureNote.Value;if(measureNote.Key<meter){var previousLength=measureLength;measureLength=lcm*(uint)value.W+lcm/value.D*value.N+1;for(var i=previousLength;i<measureLength-1;i++)measureLine+="0";foreach(var command in value.Commands)measureLine+="\n"+command+"\n";measureLine+=$"{value.Type:X}";}else{value.W-=meter;nextMeasure[value.W+(decimal)value.N/value.D]=value;nextLCM=LCM(value.D,nextLCM);}}if(measureLength>0)for(var i=measureLength;i<lcm*(uint)meter;i++)measureLine+="0";measures.Add(measureLine+",");lcm=nextLCM;measure=nextMeasure;nextMeasure=new();beats=(meter-beats)/1000;if(beats>0)measures.Add($"#DELAY {-beats*beatLength}");extraBeats=0;currentBarline=time;break;case NoteType.Don:case NoteType.Kat:if(lineData.Count>3)int.TryParse(lineData[3],out finisher);var isKat=lineData[2]==$"{(int)NoteType.Kat}";note+=$"1,{(isKat?8:0)+finisher*4}";measure[key]=(value.Commands,(isKat?2:1)+finisher*2,w,n,d);break;case NoteType.Roll:if(lineData.Count>3)decimal.TryParse(lineData[3],out length);if(lineData.Count>4)int.TryParse(lineData[4],out finisher);note+=$"2,{finisher*4},L|257:193,1,{length*totalSV*100}";measure[key]=(value.Commands,finisher+5,w,n,d);lcm=LCM(d,lcm);(w,n,d,key)=Fraction(beatLength==0?decimal.MaxValue:(extraBeats+beatTime+length-(currentBarline-timingTime)/beatLength));measure.TryGetValue(key,out value);value.Commands??=new();measure[key]=(value.Commands,8,w,n,d);break;case NoteType.Spinner:if(lineData.Count>3)decimal.TryParse(lineData[3],out length);note+=$"8,0,{Math.Round(timingTime+(beatTime+length)*beatLength)}";break;case NoteType.TimingPoint:note="";time=beatTime;if(currentBPM<0){currentBarline=time;offset=-time/1000;startingBPM=currentSV;timingTime=time;}beatTime=beatLength==0?decimal.MaxValue:((time-currentBarline)/beatLength);(w,n,d,key)=Fraction(beatLength==0?decimal.MaxValue:(extraBeats+beatTime));value=measure.GetValueOrDefault(key,(new(),0,w,n,d));value.Commands??=new();extraBeats+=beatTime;if(currentBPM!=currentSV){currentBPM=currentSV;beatLength=currentBPM==0?decimal.MaxValue:(60000/currentBPM);value.Commands.Add($"#BPMCHANGE {currentBPM}");}kiai=int.TryParse(lineData[3],out int iValue)?iValue:0;currentBarline=time;lcm=LCM(d,lcm);nextBarline=0;timingTime=time;totalSV=mapSV;currentTimingData.Add((time,false,meter,currentBPM,kiai,true));if(lineIndex>11)measure[key]=value;break;}if(note!=""){SVCheck(currentSV,time);notes.Add(note);lcm=LCM(d,lcm);}};break;case".osu":var section="";EachLine=(lineData,line)=>{if($"{line[0]}{line[^1]}"=="[]")section=line[1..^1];else switch(section){case"Difficulty":if(decimal.TryParse(FindValue("OverallDifficulty:",line),out var dValue))od=dValue;if(decimal.TryParse(FindValue("SliderMultiplier:",line),out dValue)){mapSV=dValue;totalSV=dValue;}break;case"Events":if(FindValue("//",line)=="Background and Video events")bgFileName=null;else if(lineData.Count>2)bgFileName??=lineData[2].Replace("\"","");break;case"General":audioFileName??=FindValue("AudioFilename:",line);if(int.TryParse(FindValue("PreviewTime:",line),out int iValue))previewTime=Math.Max(iValue,previewTime);break;case"HitObjects":if(lineData.Count<5)break;decimal.TryParse(lineData[2],out var time);while(currentTimingData.Count>0){var timingData=currentTimingData[0];if(currentBPM<0&&!timingData.Inherited){while(timingData.Time>time){timingData.Time-=timingData.Meter*timingData.Value;Barline(0);}timingTime=timingData.Time;}if(timingData.Time>time)break;var barlines=beatLength==0?decimal.MaxValue:((timingData.Time-timingTime)/beatLength);while(nextBarline<barlines)Barline(totalSV);if(!timingData.Inherited||timingData.Kiai!=kiai){nextBarline-=barlines;if(!timingData.Inherited){if(currentBPM>=0)nextBarline=0;beatLength=timingData.Value;currentBPM=beatLength==0?decimal.MaxValue:(60000/beatLength);meter=timingData.Meter;totalSV=mapSV;if(timingData.FirstBarline)Barline(0);}notes.Add($"{timingData.Time},{currentBPM},{(int)NoteType.TimingPoint},{timingData.Kiai}");kiai=timingData.Kiai;timingTime=timingData.Time;}if(timingData.Inherited)totalSV=timingData.Value==0?decimal.MaxValue:(-100*mapSV/timingData.Value);currentTimingData.RemoveAt(0);}var beatTime=beatLength==0?decimal.MaxValue:((time-timingTime)/beatLength);while(nextBarline<=beatTime)Barline(totalSV);int.TryParse(lineData[3],out int noteType);var note=$"{beatTime},{totalSV},";if((8&noteType)>0){if(lineData.Count>5&&decimal.TryParse(lineData[5],out dValue))note+=$"{(int)NoteType.Spinner},{(beatLength==0?decimal.MaxValue:((dValue-time)/beatLength))}";}else{int.TryParse(lineData[4],out int finisher);note+=$"{((2&noteType)>0?$"{(int)NoteType.Roll},{(currentBPM*totalSV!=0&&lineData.Count>7&&decimal.TryParse(lineData[7],out dValue)&&int.TryParse(lineData[6],out iValue)?dValue*iValue/totalSV/100:decimal.MaxValue)}":(int)((10&finisher)>0?NoteType.Kat:NoteType.Don))},{((4&finisher)>0?1:0)}";}notes.Add(note);break;case"Metadata":artist??=FindValue("Artist:",line);charter??=FindValue("Creator:",line);difficultyName??=FindValue("Version:",line);songTitle??=FindValue("Title:",line);break;case"TimingPoints":if(lineData.Count<2)break;int effects=lineData.Count>7&&int.TryParse(lineData[7],out iValue)?iValue:0;var inherited=lineData.Count>2&&lineData[6]=="0";currentTimingData.Add((decimal.TryParse(lineData[0],out dValue)?dValue:0,inherited,inherited?0:(int.TryParse(lineData[2],out iValue)?iValue:4),decimal.TryParse(lineData[1],out dValue)?dValue:0,(1&effects)>0?1:0,(8&effects)>0));break;}};break;default:continue;}foreach(var line in File.ReadLines(filePath))if(line.Length>0)EachLine(line.Split(',').ToList(),line);var savePath=folderPath+$"/{artist} - {songTitle} (TaiCloneConverter) [{difficultyName}].";if(extension==".fus"){currentTimingData.Sort();notes.Insert(0,"[HitObjects]");notes.InsertRange(0,currentTimingData.Select(nextTiming=>$"{nextTiming.Time},{(nextTiming.Value==0?(nextTiming.Inherited?decimal.MinValue:decimal.MaxValue):((nextTiming.Inherited?-100:60000)/nextTiming.Value))},{nextTiming.Meter},0,0,100,{(nextTiming.Inherited?0:1)},{nextTiming.Kiai+(nextTiming.FirstBarline?8:0)}"));notes.InsertRange(0,new[]{"osu file format v14","[General]","AudioFilename: "+audioFileName,$"PreviewTime: {previewTime}","Mode: 1","[Metadata]","Title:"+songTitle,"Artist:"+artist,"Creator:"+charter,"Version:"+difficultyName,"[Difficulty]",$"OverallDifficulty:{od}",$"SliderMultiplier:{mapSV}","[Events]","//Background and Video events",$"0,0,\"{bgFileName}\"","[TimingPoints]"});File.WriteAllLines(savePath+"osu",notes);measures.InsertRange(0,new[]{"TITLE:"+songTitle,"SUBTITLE:++"+artist,$"BPM:{startingBPM}","WAVE:"+audioFileName,$"OFFSET:{offset}",$"DEMOSTART:{previewTime/1000}","MAKER:"+charter,$"HEADSCROLL:{mapSV}","BGIMAGE:"+bgFileName,"COURSE:"+difficultyName,$"LEVEL:{od}","#START"});measures.Add("#END");File.WriteAllLines(savePath+"tja",measures);}else{if(extension==".osu")Barline(totalSV);notes.InsertRange(0,new[]{fusVersion,songTitle,$"{previewTime}",$"{od}",folderPath,difficultyName,charter,bgFileName,audioFileName,artist});filePath=savePath+"fus";File.WriteAllLines(filePath,notes);files.Enqueue(filePath);}}enum NoteType{TimingPoint,Barline,Don,Kat,Roll,Spinner}