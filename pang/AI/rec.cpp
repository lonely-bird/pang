#include<cstdio>
#include<cassert>
#include<string>
using namespace std;
FILE *fin=fopen("rec.txt","r");
FILE *fout=fopen("out.txt","w");
int main()
{
	double aTime=0.0,gTime=0.0;
	while(true)
	{
		double tmp[2];
		if(fscanf(fin,"..........AI : %lf s, Game : %lf s\n",&tmp[0],&tmp[1])!=2)break;
		aTime+=tmp[0],gTime+=tmp[1];
		int ttt;
		assert(fscanf(fin,"Episode %d avg. reward = %lf, Long-term avg. reward = %lf\n",&ttt,&tmp[0],&tmp[1])==3);
//		assert(fscanf(fin,"Episode %d avg. reward = %lf, Long-term avg. reward = %lf",&ttt,&tmp[0],&tmp[1])==3);
		printf("Episode %d...\n",ttt);
		fprintf(fout,"%.10f\n",tmp[0]);
	}
	fprintf(fout,"AI: %.3f s, Game: %.3f s\n",aTime,gTime);
	puts("done");
	return 0;
}
