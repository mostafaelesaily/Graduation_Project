import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class InputField extends StatelessWidget {
  final TextEditingController controller;
  final String? hintText, labelText;
  final IconData? icon;
  final bool? isObsecure, isRequired;
  final String? Function(String?)? validator;
  const new({
    super.key,
    required this.controller,
    required this.validator,
    this.isRequired,
    this.hintText,
    this.labelText,
    this.icon,
    this.isObsecure,
  });

  @override
  Widget build(BuildContext context) {
    return FormField<String>(
      initialValue: controller.text,
      autovalidateMode: .disabled,
      validator: validator,
      builder: (field) {
        final hasError = field.hasError;
        final borderColor = hasError ? Colors.red : const Color(0xffddecfa);

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          spacing: 4,
          children: [
            RichText(
              text: TextSpan(
                text: labelText,
                style: const TextStyle(
                  color: Colors.black,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
                children: isRequired == true
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
                controller: controller,
                obscureText: isObsecure ?? false,
                style: const TextStyle(fontSize: 12),
                onChanged: field.didChange,
                decoration: InputDecoration(
                  contentPadding: const EdgeInsets.only(left: 14),
                  constraints: BoxConstraints(minHeight: 38.w, maxHeight: 38.w),
                  fillColor: Colors.white,
                  hint: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    spacing: 12,
                    children: [
                      Icon(icon, size: 12.w),
                      Text(
                        hintText ?? '',
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
