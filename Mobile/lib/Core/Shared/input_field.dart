import 'package:flutter/material.dart';
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
    return Column(
      crossAxisAlignment: .start,
      spacing: 4,
      children: [
        RichText(
          text: TextSpan(
            text: labelText,
            style: TextStyle(
              color: Colors.black,
              fontSize: 12,
              fontWeight: .bold,
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
          height: 38,
          decoration: BoxDecoration(
            color: Colors.white,
            boxShadow: [
              BoxShadow(
                offset: Offset(0, 1),
                blurRadius: 2,
                color: Colors.black.withValues(alpha: 0.05),
              ),
            ],
            border: BoxBorder.all(color: Color(0xffddecfa)),
            borderRadius: BorderRadius.circular(12),
          ),
          child: TextFormField(
            controller: controller,
            validator: validator,
            obscureText: isObsecure ?? false,
            style: TextStyle(fontSize: 12),
            decoration: InputDecoration(
              contentPadding: EdgeInsets.only(left: 14),
              constraints: const BoxConstraints(minHeight: 38, maxHeight: 38),
              fillColor: Colors.white,
              hint: Row(
                crossAxisAlignment: .center,
                spacing: 12,
                children: [
                  Icon(icon, size: 12),
                  Text(hintText ?? '', style: TextStyle(fontSize: 12)),
                ],
              ),

              border: OutlineInputBorder(
                borderSide: BorderSide(color: Color(0xffddecfa)),
                borderRadius: BorderRadius.circular(12),
              ),
              enabledBorder: OutlineInputBorder(
                borderSide: BorderSide(color: Color(0xffddecfa)),
                borderRadius: BorderRadius.circular(12),
              ),
              focusedBorder: OutlineInputBorder(
                borderSide: BorderSide(color: Color(0xffddecfa)),
                borderRadius: BorderRadius.circular(12),
              ),
              errorBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),
        ),
      ],
    );
  }
}
