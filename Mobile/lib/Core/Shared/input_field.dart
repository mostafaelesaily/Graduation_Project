import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class InputField extends StatefulWidget {
  final TextEditingController controller;
  final String? hintText, labelText;
  final IconData? icon;
  final bool? isRequired, issuffixShown;
  bool isObsecure;
  final String? Function(String?)? validator;
  new({
    super.key,
    required this.controller,
    required this.validator,
    this.isRequired,
    this.hintText,
    this.labelText,
    this.icon,
    this.isObsecure = false,
    this.issuffixShown,
  });

  @override
  State<InputField> createState() => _InputFieldState();
}

class _InputFieldState extends State<InputField> {
  @override
  Widget build(BuildContext context) {
    return FormField<String>(
      initialValue: widget.controller.text,
      autovalidateMode: .disabled,
      validator: widget.validator,
      builder: (field) {
        final hasError = field.hasError;
        final borderColor = hasError ? Colors.red : const Color(0xffddecfa);

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          spacing: 4,
          children: [
            RichText(
              text: TextSpan(
                text: widget.labelText,
                style: const TextStyle(
                  color: Colors.black,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
                children: widget.isRequired == true
                    ? [
                        TextSpan(
                          text: ' *',
                          style: TextStyle(color: ColorsManager.primaryColor),
                        ),
                      ]
                    : [],
              ),
            ),
            Container(
              height: 38.w,
              decoration: BoxDecoration(
                color: Colors.white,
                boxShadow: [
                  BoxShadow(
                    offset: const Offset(0, 1),
                    blurRadius: 2,
                    color: Colors.black.withValues(alpha: 0.05),
                  ),
                ],
                borderRadius: BorderRadius.circular(12),
              ),
              child: TextFormField(
                controller: widget.controller,
                obscureText: widget.isObsecure ?? false,
                style: const TextStyle(fontSize: 12),
                onChanged: field.didChange,
                decoration: InputDecoration(
                  suffixIconConstraints: BoxConstraints(
                    minWidth: 12,
                    minHeight: 12,
                  ),
                  suffix: widget.issuffixShown == true
                      ? Transform.translate(
                          offset: Offset(0, 2.5),
                          child: IconButton(
                            onPressed: () => setState(() {
                              widget.isObsecure = !widget.isObsecure;
                            }),
                            style: IconButton.styleFrom(
                              minimumSize: Size(12.w, 12.w),
                              maximumSize: Size(30.w, 30.w),
                              padding: EdgeInsets.only(
                                right: 14,
                                top: 10,
                                bottom: 10,
                              ),
                              visualDensity: .compact,
                              tapTargetSize: .shrinkWrap,
                              splashFactory: null,
                              overlayColor: Colors.transparent,
                              iconSize: 12,
                            ),
                            icon: Icon(
                              widget.isObsecure == true
                                  ? Icons.visibility_outlined
                                  : Icons.visibility_off_outlined,
                              size: 12,
                            ),
                          ),
                        )
                      : null,
                  contentPadding: const EdgeInsets.only(left: 14),
                  constraints: BoxConstraints(minHeight: 38.w, maxHeight: 38.w),
                  fillColor: Colors.white,
                  hint: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    spacing: 12,
                    children: [
                      Icon(widget.icon, size: 12.w),
                      Text(
                        widget.hintText ?? '',
                        style: const TextStyle(fontSize: 12),
                      ),
                    ],
                  ),
                  border: OutlineInputBorder(
                    borderSide: BorderSide(color: borderColor),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  enabledBorder: OutlineInputBorder(
                    borderSide: BorderSide(color: borderColor),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderSide: BorderSide(color: borderColor),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  errorBorder: OutlineInputBorder(
                    borderSide: BorderSide(color: borderColor),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  focusedErrorBorder: OutlineInputBorder(
                    borderSide: BorderSide(color: borderColor),
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),
            if (hasError)
              Padding(
                padding: const EdgeInsets.only(left: 4),
                child: Text(
                  field.errorText!,
                  style: TextStyle(color: Colors.red, fontSize: 10.sp),
                ),
              ),
          ],
        );
      },
    );
  }
}
