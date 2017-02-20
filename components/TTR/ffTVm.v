module ff3TV(clk, inD, outD, ctr); // FF-block with Voter (325.0 MHz/8 HW )
	input clk; // clock signal
	input inD; //data input
	output  outD;//data output
	input [1:0]ctr; //control clock inputs for Time Redundancy
	
	reg d1;// 3 register for TripleTimeRed (TTR)s
	reg d2;
	reg d3;
	
	reg keepA;
	reg keepB;
	
	
	
	wire muxA/*synthesis syn_preserve = 1*/;
	wire muxB/*synthesis syn_preserve = 1*/;
	assign muxA= (ctr[0])? d2:keepA;
	assign muxB= (ctr[1])? d1:keepB;
	
	//voting part
	wire votA, votB/*synthesis syn_preserve = 1*/;
	
	voter1 VoterA(votA, d3, muxA, muxB)/*synthesis syn_preserve = 1*/;
	voter1 VoterB(votB, d3, muxA, muxB)/*synthesis syn_preserve = 1*/;
	
	assign outD=votA;
	
	always @(posedge clk)
	begin
		d1<=inD;
		d2<=d1;
		d3<=d2;
		keepA<=votA;
		keepB<=votB;
	end
	
endmodule