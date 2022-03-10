; macros (from https://github.com/tandasat/HyperPlatform/blob/master/HyperPlatform/Arch/x64/x64.asm)

; Saves all general purpose registers to the stack
PUSHAQ MACRO
    push    rax
    push    rcx
    push    rdx
    push    rbx
    push    -1      ; dummy for rsp
    push    rbp
    push    rsi
    push    rdi
    push    r8
    push    r9
    push    r10
    push    r11
    push    r12
    push    r13
    push    r14
    push    r15
ENDM

; Loads all general purpose registers from the stack
POPAQ MACRO
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     r11
    pop     r10
    pop     r9
    pop     r8
    pop     rdi
    pop     rsi
    pop     rbp
    add     rsp, 8    ; dummy for rsp
    pop     rbx
    pop     rdx
    pop     rcx
    pop     rax
ENDM


.CODE

EXTERN DumpCallback:PROC


DO_CALLBACK_RCX_U1ARRAY MACRO
	pushfq
	PUSHAQ

	sub rsp, 20h
	mov edx, dword ptr [rcx+8h]
	add rcx, 10h
	call DumpCallback
	add rsp, 20h

	POPAQ
	popfq
ENDM


LoadImageStub_Mscorwks PROC
	DO_CALLBACK_RCX_U1ARRAY
	EXTERN LoadImageOriginal_Mscorwks:QWORD
	jmp [LoadImageOriginal_Mscorwks]
LoadImageStub_Mscorwks ENDP


LoadImageStub_CLR PROC
	DO_CALLBACK_RCX_U1ARRAY
	EXTERN LoadImageOriginal_CLR:QWORD
	jmp [LoadImageOriginal_CLR]
LoadImageStub_CLR ENDP

END
