.MODEL FLAT, C

.CODE

EXTERN DumpCallback:PROC


DO_CALLBACK_ECX_U1ARRAY MACRO
	pushfd
	pushad

	push dword ptr [ecx+4h]
	add ecx, 8h
	push ecx
	call DumpCallback
	add esp, 8h

	popad
	popfd
ENDM


LoadImageStub_Mscorwks PROC
	DO_CALLBACK_ECX_U1ARRAY
	EXTERN LoadImageOriginal_Mscorwks:DWORD
	jmp [LoadImageOriginal_Mscorwks]
LoadImageStub_Mscorwks ENDP


LoadImageStub_CLR PROC
	DO_CALLBACK_ECX_U1ARRAY
	EXTERN LoadImageOriginal_CLR:DWORD
	jmp [LoadImageOriginal_CLR]
LoadImageStub_CLR ENDP

END
