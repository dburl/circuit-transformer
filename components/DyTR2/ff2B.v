//---------------------------------------------------------------------------------------------------------------
//Author: 				Burlyaev Dmitry
//Affiliation: 			INRIA, Grenoble, France
//Release Date: 		22/09/2013
//Contact Information: 	burlyaev.dmitry {æt} gmail.com
//---------------------------------------------------------------------------------------------------------------

// Dff replacement for TDMR with self recovery-> so actually it's 2 period+ max 2 delay
module ff2DB(clk, inp, out, // casual Dff interface
				modeS, fail); // added Dff-block interface to support  dynamicity
	input 	clk; // clock signal
	input 	inp; //data input
	output  out;//data output
	input modeS; //controls circuit image saving procedure
	output fail;//indicate that in this block error is detected
	
	// Registers
	reg d1;// 1st of 2 register for Time Double Redundancy(TDR)
	reg d2;// 2st of 2 register for Time Double Redundancy(TDR)
					
	//Assignments
	
	assign out= (modeS)? d1:d2;
	assign fail= (out==d1)? 0:1;
		
	//Process							
	always @(posedge clk)
	begin
		d1<=inp;
		d2<=d1;
	end
endmodule