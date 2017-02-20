//---------------------------------------------------------------------------------------------------------------
//Author: 				Burlyaev Dmitry
//Affiliation: 			INRIA, Grenoble, France
//Release Date: 		22/09/2013
//Contact Information: 	burlyaev.dmitry {æt} gmail.com
//---------------------------------------------------------------------------------------------------------------

// Dff replacement for TDMR with self recovery-> so actually it's 2 period+ max 2 delay
module ff2B(clk, inp, out, // casual Dff interface
				save, rollBack, fail); // added Dff-block interface to support self-recovery TDR (Time Double Redundancy)
	input 	clk; // clock signal
	input 	inp; //data input
	output  out;//data output
	input save; //controls circuit image saving procedure
	input rollBack; // switch on the speed up mode: d1&d2 are set simultaniously
	output fail;//indicate that in this block error is detected
	
	// Registers
	reg d1;// 1st of 2 register for Time Double Redundancy(TDR)
	reg d2;// 2st of 2 register for Time Double Redundancy(TDR)
				
	reg check; // save same state if no error detected within 3 clk (some clk in general) propagate to recov
	reg recov; // for keeping the value 1 more clock cycle- useful during the recovery procedure

	//Assignments
	assign fail= (d1!=d2); // error detection
	wire mu; // after MuxA
	assign mu= (save)? recov:d1;
	
	//Definition of Output Value
	assign out= (rollBack)? mu:d2;
	//Process							
	always @(posedge clk)
	begin
		d1<=inp;
		d2<=d1;
		if (save)
			begin
				check<=inp;
				recov<=check;
			end
	end
endmodule