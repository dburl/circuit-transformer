//---------------------------------------------------------------------------------------------------------------
//Author: 				Burlyaev Dmitry
//Affiliation: 			INRIA, Grenoble, France
//Release Date: 		22/09/2013
//Contact Information: 	burlyaev.dmitry {æt} gmail.com
//---------------------------------------------------------------------------------------------------------------

// Dff replacement for TDMR with self recovery-> so actually it's 2 period+ max 2 delay
module ff3DB(clk, inp, out, // casual Dff interface
				modeS, fetchA, fail); // added Dff-block interface to support  dynamicity
	input 	clk; // clock signal
	input 	inp; //data input
	output  out;//data output
	input modeS; //controls circuit image saving procedure
	input fetchA; // switch on the speed up mode: d1&d2 are set simultaniously
	output fail;//indicate that in this block error is detected
	
	// Registers
	reg d1;// 1st of 2 register for Time Double Redundancy(TDR)
	reg d2;// 2st of 2 register for Time Double Redundancy(TDR)
	reg d3;// 2st of 2 register for Time Double Redundancy(TDR)
	reg s;// 2st of 2 register for Time Double Redundancy(TDR)
				
	//Assignments
	wire muxA, muxB;// /*synthesis syn_preserve = 1*/;
	assign muxA= (fetchA)? d1:s;
	assign muxB= (modeS)? inp:d1;
	
	wire muxC;// [B] new FT scheme
	assign muxC = (modeS)? s:d3;// [B] new FT scheme
	
	//voterF VoterA(out, fail, d2, d3, muxA); // /*synthesis syn_preserve = 1*/;
	voterF VoterA(out, fail, muxC, d2, muxA); // [B] new FT scheme
	
	//Process							
	always @(posedge clk)
	begin
		d1<=inp;
		d2<=muxB;
		d3<=d2;
		s<=d3;
	end
endmodule