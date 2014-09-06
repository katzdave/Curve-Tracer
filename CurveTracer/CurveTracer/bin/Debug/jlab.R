#library(extrafont)

files =  list.files(path = ".", pattern = "temp[1-9]\\.csv")
l <- length(files)

pdf('tmp.pdf',width=7,height=4.5,family="Palatino")

data <- read.table("temp0.csv",header=F,sep=",")

maxi = max(data[,2]);
mini = min(data[,2]);

if (l>0){
   for ( ii in c(1:l)) {
   data <- read.table(files[ii],header=F,sep=",")
   maxi = max(c(maxi,max(data[,2])))
   mini = min(c(mini,min(data[,2])))
   }
}

data <- read.table("temp0.csv",header=F,sep=",")
if (l>0) {
	leg <- read.table("params.csv",header=F,sep=",")
}
plot(data,type="l",
     xlab ="Voltage (V)",
     ylab="Current (mA)",las=1,
     ylim = c(mini,maxi))
title('IV Characteristic')
grid(lwd=1,col="darkgray",lty=2)



if (l>0) {

for ( ii in c(1:l)) {
  data <- read.table(files[ii],header=F,sep=",")
  lines(data,col=ii+1)
 }
}

if (l>0) {
  legend(x="bottomright",y=NULL,leg[,1],lty=c(1,1),col =c(1:(l+1)),bg="white")
}
#embed_fonts("tmp.pdf")
dev.off()

file.rename("tmp.pdf","temp.pdf")
