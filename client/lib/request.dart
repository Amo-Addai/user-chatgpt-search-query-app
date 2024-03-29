import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';


const String apiUrl = 'https://localhost:7028/'; // backend-api

Future<dynamic> getFromLocalStorage(String key) async {
  dynamic result;
  final prefs = await SharedPreferences.getInstance();
  if (['token', 'user'].contains(key)) {
    result = prefs.getString(key);
    if (key == 'user') result = jsonDecode(result);
  }
  return result;
}

Future<bool> removeFromLocalStorage(String key) async {
  bool result = false;
  final prefs = await SharedPreferences.getInstance();
  if (['token', 'user'].contains(key)) {
    prefs.remove(key);
    result = true;
  }
  return result;
}

Future<Map<String, dynamic>?> loginUser(String username, String password) async {
  final response = await http.post(
    Uri.parse('${apiUrl}auth/login'),
    headers: <String, String>{
      'Content-Type': 'application/json',
    },
    body: jsonEncode(<String, String>{
      'Username': username,
      'Password': password,
    }),
  );

  if (response.statusCode == 200) {
    // print('Body: ${jsonEncode(response.body)}');

    // Extract token from response
    Map<String, dynamic> body = jsonDecode(response.body);
    String token = body.remove('token');

    // Save token to local storage
    final prefs = await SharedPreferences.getInstance();
    prefs.setString('token', token);
    prefs.setString('user', jsonEncode((body)));

    // todo: Output these results (or remove)
    // print('Token: $token');
    // print('Body: ${jsonEncode(body)}');

    return body;

  } else {
    print('Request Error');
  }
  return null;
}

Future<Map<String, dynamic>?> getUser() async {
  String token = await getFromLocalStorage('token');
  Map<String, dynamic>? user = await getFromLocalStorage('user');
  // print('Token: $token | User: ${jsonEncode(user)}');

  if (user != null) {
    final response = await http.get(
      Uri.parse('${apiUrl}user/${user['id']}'),
      headers: <String, String>{
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 200) {
      // print('Body: ${jsonEncode(response.body)}');
      Map<String, dynamic> body = jsonDecode(response.body);
      return body;
    } else {
      print('Request Error');
    }
  }
  return null;
}

Future<Map<String, dynamic>?> postQuery(String query) async {
  String token = await getFromLocalStorage('token');
  Map<String, dynamic>? user = await getFromLocalStorage('user');
  // print('Token: $token | User: ${user?['id']} | Query: $query');

  final response = await http.post(
    Uri.parse('${apiUrl}query'),
    headers: <String, String>{
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    },
    body: jsonEncode(<String, String>{
      'UserId': '${user?['id']}', // todo: remove when not needed anymore
      'Query': query
    }),
  );

  if (response.statusCode == 200) {
    // print('Body: ${jsonEncode(response.body)}');
    Map<String, dynamic> body = jsonDecode(response.body);
    return body;
  } else {
    print('Request Error');
  }
  return null;
}

Future<Map<String, dynamic>?> getUserQueries() async {
  String token = await getFromLocalStorage('token');
  Map<String, dynamic>? user = await getFromLocalStorage('user');

  if (user != null) {
    final response = await http.get(
      Uri.parse('${apiUrl}query/${user['id']}'),
      headers: <String, String>{
        'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 200) {
      // print('Body: ${jsonEncode(response.body)}');
      Map<String, dynamic> body = jsonDecode(response.body);
      return body;
    } else {
      print('Request Error');
    }
  }
  return null;
}

Future<void> logout() async {
  bool response = await removeFromLocalStorage('token');
  response = await removeFromLocalStorage('user');
  // work with response, if required
}
