// Dff replacement for TTMR
module ff3T(clk, inD, outD, ctr); // Flip-Flop Block wo voter( constraint  slack /5 HW)
	input clk; // clock signal
	input inD; //data input
	output  outD;//data output
	input [1:0]ctr; //control clock inputs for Time Redundancy
	
	reg d1;// 3 register for TripleTimeRed (TTR)s
	reg d2;
	reg d3;
	
	assign outD=d3;
	
	always @(posedge clk)
	begin
		d1<=inD;
		d2<=d1;
		d3<=d2;
	end
endmodule








