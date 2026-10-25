import 'package:flutter/material.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class TextStyles {
  static const TextStyle fadedText = TextStyle(
    color: ColorsManager.fadedText,
    fontSize: 12,
  );

  static const TextStyle authTitle = TextStyle(fontSize: 23, fontWeight: .bold);
  static const TextStyle buttonText = TextStyle(
    color: Colors.white,
    fontSize: 14,
    fontWeight: .bold,
  );
}
