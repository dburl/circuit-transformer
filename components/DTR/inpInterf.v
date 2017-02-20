//Input Block
module inpBlock (clk, inp, inpNew, buffRead);
	input 	clk; // clock signal
	input inp;//original primary input of the circuit
	output inpNew; //new input for the original circuit
	input buffRead; //global control signal
	
	reg delOne;
	reg delTwo;
	
	assign inpNew= buffRead?delTwo:inp;
	
	always @(posedge clk)
	begin
		delOne<=inp;
		delTwo<=delOne;
	end
endmodule
	
