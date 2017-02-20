// Single Voter Module
module ctr3T(clk, reset, ctr); // counter 0-2- provides ctr signals for TTR
	input clk; //global synch. clock
	input reset; //global reset sygnal
	output [1:0]ctr; //control outpus for Time Redundancy
	
	reg [1:0] counter;
	
	assign ctr[0]= (!counter[1]) && (!counter[0]);
	assign ctr[1]=  (!counter[1]) && (!counter[0]);
	
	always @ (posedge clk or posedge reset) begin // (ProASIC:632.6 Mhz/15 HW) - really cool
        if (reset)
            counter <= 0;
        else
            case (counter)
                0: counter <= 1;
                1: counter <= 2;
                2: counter <= 0;
            endcase // case (state)
    end // always @ (posedge clk or posedge reset)
	
/* 	always @(posedge clk) //(ProASIC3: 369.2 MHz/20 HW.)
	begin
		if ((reset)||ctr[2]) 
			counter <= 0;
		else 
			counter <= counter + 1;	
	end */
endmodule

//NOTE; such voting will save only agains 1 SEU, synchronization is not established
//TODO: create re-synchronization between counters-> should be efficient
//ctr[3]||reset dropped frequency from 361 to <150Mhz-> so no the best choice- 
//[TODO] re-check

module ctr3Tmr(clk, reset, ctr); // TMR counter for TTR ctr signals
	input  clk; //global synch. clock
	input  [2:0]reset; //global reset signal
	output [1:0]ctr; //control output for Time Redundancy
	
	wire [6:0]ctrL;

	genvar i;
	generate
		for (i=0;i<3; i=i+1)
		begin: ripple
			ctr3T count(clk, reset[i],ctrL[1+2*i:0+2*i])/*synthesis syn_preserve = 1*/; 
		end
	endgenerate
	
	genvar i;
	generate
	for (i=0;i<2; i=i+1)
		begin: ripple
			voter1 V(ctr[i],ctrL[0+i],ctrL[2+i],ctrL[4+i])/*synthesis syn_preserve = 1*/; 
		end
	endgenerate
endmodule	
	
	